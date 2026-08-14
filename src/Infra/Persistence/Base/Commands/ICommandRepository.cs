namespace Admiral.Booking.Infra.Persistence.Base.Commands;

using Admiral.Booking.AppCore.Domain.Base;
using System.Linq.Expressions;

public interface ICommandRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TId : struct, IComparable, IComparable<TId>, IEquatable<TId>
{
    void Delete(TId id);
    void DeleteGraph(TId id);
    void Delete(TEntity entity);
    void Insert(TEntity entity);
    Task InsertAsync(TEntity entity);
    TEntity Get(TId id);
    Task<TEntity> GetAsync(TId id);
    TEntity GetGraph(TId id);
    Task<TEntity> GetGraphAsync(TId id);
    bool Exists(Expression<Func<TEntity, bool>> expression);
    Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression);
    int Commit();
    Task<int> CommitAsync();
}
