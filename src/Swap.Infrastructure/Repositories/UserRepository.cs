using Microsoft.EntityFrameworkCore;
using Swap.Domain.Entities.Users;
using Swap.Infrastructure.Database;

namespace Swap.Infrastructure.Repositories;

internal sealed class UserRepository(
    ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return await context.Users.SingleOrDefaultAsync(u => u.Id == id, cancellationToken);
    }

    public void Insert(User user)
    {
        context.Users.Add(user);
    }
}
