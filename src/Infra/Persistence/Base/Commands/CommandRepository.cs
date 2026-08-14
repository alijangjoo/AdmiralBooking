namespace Admiral.Booking.Infra.Persistence.Base.Commands;

using Admiral.Booking.AppCore.Domain.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

public class CommandRepository<TEntity, TDbContext, TId> : ICommandRepository<TEntity, TId>
    where TEntity : AggregateRoot<TId>
    where TDbContext : CommandDbContext
    where TId : struct, IComparable, IComparable<TId>, IEquatable<TId>
{
    protected readonly TDbContext _dbContext;

    public CommandRepository(TDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Delete(TId id)
    {
        var entity = _dbContext.Set<TEntity>().Find(id);
        if (entity is not null)
            _dbContext.Set<TEntity>().Remove(entity);
    }

    public void Delete(TEntity entity) => _dbContext.Set<TEntity>().Remove(entity);

    public void DeleteGraph(TId id)
    {
        var entity = GetGraph(id);
        if (entity is not null && !entity.Id.Equals(default(TId)))
            _dbContext.Set<TEntity>().Remove(entity);
    }

    public void Insert(TEntity entity) => _dbContext.Set<TEntity>().Add(entity);

    public async Task InsertAsync(TEntity entity) => await _dbContext.Set<TEntity>().AddAsync(entity);

    public TEntity Get(TId id) => _dbContext.Set<TEntity>().Find(id)!;

    public async Task<TEntity> GetAsync(TId id) => (await _dbContext.Set<TEntity>().FindAsync(id))!;

    public TEntity GetGraph(TId id)
    {
        var graphPath = _dbContext.GetIncludePaths(typeof(TEntity));
        IQueryable<TEntity> query = _dbContext.Set<TEntity>().AsQueryable();
        foreach (var item in graphPath)
            query = query.Include(item);

        return query.FirstOrDefault(c => c.Id.Equals(id))!;
    }

    public async Task<TEntity> GetGraphAsync(TId id)
    {
        var graphPath = _dbContext.GetIncludePaths(typeof(TEntity));
        IQueryable<TEntity> query = _dbContext.Set<TEntity>().AsQueryable();
        foreach (var item in graphPath)
            query = query.Include(item);

        return (await query.FirstOrDefaultAsync(c => c.Id.Equals(id)))!;
    }

    public bool Exists(Expression<Func<TEntity, bool>> expression) => _dbContext.Set<TEntity>().Any(expression);

    public async Task<bool> ExistsAsync(Expression<Func<TEntity, bool>> expression)
        => await _dbContext.Set<TEntity>().AnyAsync(expression);

    public int Commit() => _dbContext.SaveChanges();

    public Task<int> CommitAsync() => _dbContext.SaveChangesAsync();
}

public class CommandRepository<TEntity, TDbContext> : CommandRepository<TEntity, TDbContext, int>
    where TEntity : AggregateRoot
    where TDbContext : CommandDbContext
{
    public CommandRepository(TDbContext dbContext) : base(dbContext)
    {
    }
}
