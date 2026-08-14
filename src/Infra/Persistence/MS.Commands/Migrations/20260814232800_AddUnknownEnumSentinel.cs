using Admiral.Booking.Infra.Persistence.MS.Commands;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Admiral.Booking.Infra.Persistence.MS.Commands.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(AppCommandDbContext))]
    [Migration("20260814232800_AddUnknownEnumSentinel")]
    public partial class AddUnknownEnumSentinel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_CapacityReservations_Booking_Voyage_Active",
                table: "CapacityReservations");

            migrationBuilder.Sql("UPDATE Bookings SET Status = Status + 1;");
            migrationBuilder.Sql("UPDATE Voyages SET Status = Status + 1;");
            migrationBuilder.Sql("UPDATE CapacityReservations SET Status = Status + 1;");
            migrationBuilder.Sql("UPDATE OutboxMessages SET Status = Status + 1;");
            migrationBuilder.Sql("UPDATE IdempotencyRecords SET Status = Status + 1;");

            migrationBuilder.CreateIndex(
                name: "UX_CapacityReservations_Booking_Voyage_Active",
                table: "CapacityReservations",
                columns: new[] { "BookingId", "VoyageId" },
                unique: true,
                filter: "[Status] = 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_CapacityReservations_Booking_Voyage_Active",
                table: "CapacityReservations");

            migrationBuilder.Sql("UPDATE Bookings SET Status = Status - 1 WHERE Status > 0;");
            migrationBuilder.Sql("UPDATE Voyages SET Status = Status - 1 WHERE Status > 0;");
            migrationBuilder.Sql("UPDATE CapacityReservations SET Status = Status - 1 WHERE Status > 0;");
            migrationBuilder.Sql("UPDATE OutboxMessages SET Status = Status - 1 WHERE Status > 0;");
            migrationBuilder.Sql("UPDATE IdempotencyRecords SET Status = Status - 1 WHERE Status > 0;");

            migrationBuilder.CreateIndex(
                name: "UX_CapacityReservations_Booking_Voyage_Active",
                table: "CapacityReservations",
                columns: new[] { "BookingId", "VoyageId" },
                unique: true,
                filter: "[Status] = 0");
        }
    }
}
