using Microsoft.EntityFrameworkCore;
using UserService.Domain.Entities;
using UserService.Domain.Repositories;
using UserService.Domain.ValueObjects;
using UserService.Infrastructure.Data;

namespace UserService.Infrastructure.Repositories;

public sealed class UserRepository(UserDbContext dbContext) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await dbContext.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

    public async Task<User?> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
        => await dbContext.Users.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

    public async Task<(IReadOnlyList<User> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = dbContext.Users.AsNoTracking();
        var total = await query.CountAsync(cancellationToken);
        var items = await query.OrderBy(u => u.RegisteredAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(cancellationToken);
        return (items.AsReadOnly(), total);
    }

    public async Task AddAsync(User user, CancellationToken cancellationToken = default)
        => await dbContext.Users.AddAsync(user, cancellationToken);

    public void Update(User user) => dbContext.Users.Update(user);

    public async Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
        => await dbContext.Users.AnyAsync(u => u.Email == email, cancellationToken);
}
