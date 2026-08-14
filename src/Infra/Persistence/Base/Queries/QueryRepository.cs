namespace Admiral.Booking.Infra.Persistence.Base.Queries;

public class QueryRepository<TDbContext>(TDbContext dbContext) : IQueryRepository
    where TDbContext : QueryDbContext
{
    protected readonly TDbContext _dbContext = dbContext;
}
