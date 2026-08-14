namespace Admiral.Booking.AppCore.AppServices.Bookings.Commands.Confirm;

using Admiral.Booking.AppCore.Domain.Base.Exceptions;
using Admiral.Booking.AppCore.Domain.Base.Rules;
using Admiral.Booking.AppCore.Domain.Bookings.Entities;
using Admiral.Booking.AppCore.Domain.Bookings.Events;
using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Entities;
using Admiral.Booking.AppCore.Domain.IdempotencyRecords.Enums;
using Admiral.Booking.AppCore.Domain.OutboxMessages.Entities;
using Admiral.Booking.AppCore.Shared.Base;
using Admiral.Booking.AppCore.Shared.Bookings.Commands;
using Admiral.Booking.AppCore.Shared.Bookings.Commands.Confirm;
using Admiral.Booking.AppCore.Shared.Bookings.IntegrationEvents;
using Admiral.Booking.AppCore.Shared.Bookings.Services;
using Admiral.Booking.AppCore.Shared.IdempotencyRecords.Commands;
using Admiral.Booking.AppCore.Shared.OutboxMessages.Commands;
using Admiral.Booking.AppCore.Shared.Voyages.Commands;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

public class ConfirmBookingCommandHandler(
    IBookingCommandRepository bookingCommandRepository,
    IVoyageCommandRepository voyageCommandRepository,
    IIdempotencyRecordCommandRepository idempotencyRecordCommandRepository,
    IOutboxMessageCommandRepository outboxMessageCommandRepository,
    IBookingConfirmationService bookingConfirmationService,
    ILogger<ConfirmBookingCommandHandler> logger)
    : IRequestHandler<ConfirmBookingCommand, CommandResult<ConfirmBookingCommandResult>>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public async Task<CommandResult<ConfirmBookingCommandResult>> Handle(
        ConfirmBookingCommand request,
        CancellationToken cancellationToken)
    {
        var fingerprint = ComputeFingerprint(request);
        var existing = await idempotencyRecordCommandRepository.FindByKeyAsync(request.IdempotencyKey, cancellationToken);
        if (existing is not null)
            return HandleExistingRecord(existing, fingerprint);

        try
        {
            var result = await ConfirmCoreAsync(request, fingerprint, cancellationToken);
            await bookingCommandRepository.CommitAsync();
            return CommandResult<ConfirmBookingCommandResult>.Success(result);
        }
        catch (BusinessRuleValidationException ex)
        {
            logger.LogWarning(
                "Booking confirmation rejected. BookingId={BookingId} ErrorCode={ErrorCode}",
                request.BookingId,
                ex.ErrorCode);

            return CommandResult<ConfirmBookingCommandResult>.Failure(ex.Message, ex.ErrorCode);
        }
        catch (DomainException ex)
        {
            logger.LogWarning(
                ex,
                "Booking confirmation domain failure. BookingId={BookingId} ErrorCode={ErrorCode}",
                request.BookingId,
                ex.ErrorCode);

            return CommandResult<ConfirmBookingCommandResult>.Failure(ex.Message, ex.ErrorCode);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            logger.LogWarning(
                ex,
                "Optimistic concurrency conflict while confirming booking. BookingId={BookingId}",
                request.BookingId);

            return CommandResult<ConfirmBookingCommandResult>.Failure(
                "The voyage capacity was updated by another request. Retry is safe with the same idempotency key.",
                "CONCURRENCY_CONFLICT");
        }
        catch (DbUpdateException ex) when (IsIdempotencyConflict(ex))
        {
            var raced = await idempotencyRecordCommandRepository.FindByKeyAsync(request.IdempotencyKey, cancellationToken);
            if (raced is not null)
                return HandleExistingRecord(raced, fingerprint);

            return CommandResult<ConfirmBookingCommandResult>.Failure(
                "A concurrent request with the same idempotency key is already in progress.",
                "IDEMPOTENCY_IN_PROGRESS");
        }
    }

    private async Task<ConfirmBookingCommandResult> ConfirmCoreAsync(
        ConfirmBookingCommand request,
        string fingerprint,
        CancellationToken cancellationToken)
    {
        var booking = await bookingCommandRepository.FindAsync(request.BookingId, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Booking), request.BookingId);

        if (request.ExpectedVersion.HasValue && request.ExpectedVersion.Value != booking.Version)
        {
            throw new BusinessRuleValidationException(
                $"Booking version mismatch. Expected {request.ExpectedVersion}, actual {booking.Version}.",
                "CONCURRENCY_CONFLICT");
        }

        var voyage = await voyageCommandRepository.FindGraphAsync(booking.VoyageId, cancellationToken)
            ?? throw new EntityNotFoundException(typeof(Admiral.Booking.AppCore.Domain.Voyages.Entities.Voyage), booking.VoyageId);

        logger.LogInformation(
            "Confirming booking. BookingId={BookingId} VoyageId={VoyageId} CorrelationId={CorrelationId} RequestedUnits={RequestedUnits}",
            booking.Id,
            voyage.Id,
            request.CorrelationId,
            booking.RequestedUnits.Value);

        var outcome = bookingConfirmationService.Confirm(booking, voyage);
        EnqueueIntegrationEvent(booking, request.CorrelationId);

        var result = new ConfirmBookingCommandResult
        {
            BookingId = booking.Id,
            Status = booking.Status.ToString(),
            VoyageId = booking.VoyageId,
            ReservationId = outcome.ReservationId,
            RequestedUnits = booking.RequestedUnits.Value,
            ConfirmedAt = booking.ConfirmedAt,
            Version = booking.Version,
            Replayed = false
        };

        var record = IdempotencyRecord.Start(Guid.NewGuid(), request.IdempotencyKey, fingerprint, request.BookingId);
        record.Complete(JsonSerializer.Serialize(result, JsonOptions));
        await idempotencyRecordCommandRepository.InsertAsync(record);

        return result;
    }

    private void EnqueueIntegrationEvent(Booking booking, string? correlationId)
    {
        var confirmed = booking.GetEvents().OfType<BookingConfirmed>().LastOrDefault();
        if (confirmed is null)
            return;

        var integrationEvent = new BookingConfirmedIntegrationEvent
        {
            EventId = Guid.NewGuid(),
            BookingId = confirmed.BookingId,
            CustomerId = confirmed.CustomerId,
            VoyageId = confirmed.VoyageId,
            ReservationId = confirmed.ReservationId,
            ReservedUnits = confirmed.ReservedUnits,
            ConfirmedAt = confirmed.ConfirmedAt,
            CorrelationId = correlationId
        };

        var payload = JsonSerializer.Serialize(integrationEvent, JsonOptions);
        var outbox = OutboxMessage.Create(
            integrationEvent.EventId,
            BookingConfirmedIntegrationEvent.EventType,
            payload,
            integrationEvent.EventId);

        outboxMessageCommandRepository.Insert(outbox);
    }

    private static CommandResult<ConfirmBookingCommandResult> HandleExistingRecord(
        IdempotencyRecord existing,
        string fingerprint)
    {
        if (!existing.HasSameFingerprint(fingerprint))
        {
            return CommandResult<ConfirmBookingCommandResult>.Failure(
                "The idempotency key was reused with a different request payload.",
                "IDEMPOTENCY_KEY_CONFLICT");
        }

        if (existing.Status == IdempotencyStatus.Started)
        {
            return CommandResult<ConfirmBookingCommandResult>.Failure(
                "A request with this idempotency key is already being processed.",
                "IDEMPOTENCY_IN_PROGRESS");
        }

        if (existing.Status == IdempotencyStatus.Failed)
        {
            return CommandResult<ConfirmBookingCommandResult>.Failure(
                existing.ResponsePayload ?? "The original request failed.",
                existing.ErrorCode ?? "CONFIRMATION_FAILED");
        }

        var replayed = JsonSerializer.Deserialize<ConfirmBookingCommandResult>(existing.ResponsePayload ?? "{}", JsonOptions)
            ?? new ConfirmBookingCommandResult();
        replayed.Replayed = true;
        return CommandResult<ConfirmBookingCommandResult>.Success(replayed);
    }

    private static bool IsIdempotencyConflict(DbUpdateException exception)
    {
        var message = exception.InnerException?.Message ?? exception.Message;
        return message.Contains("UX_IdempotencyRecords_Key", StringComparison.OrdinalIgnoreCase)
               || message.Contains("unique", StringComparison.OrdinalIgnoreCase);
    }

    private static string ComputeFingerprint(ConfirmBookingCommand request)
    {
        var raw = $"{request.BookingId:N}|{request.ExpectedVersion?.ToString() ?? "_"}";
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
        return Convert.ToHexString(hash);
    }
}
