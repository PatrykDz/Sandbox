using UserService.Domain.Repositories;

namespace UserService.Infrastructure.Data;

public sealed class UserUnitOfWork(UserDbContext dbContext) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => dbContext.SaveChangesAsync(cancellationToken);
}
