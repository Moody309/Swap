using Microsoft.EntityFrameworkCore;
using Swap.Application.Abstractions.Data;
using Swap.Domain.Entities.Users;

namespace Swap.Infrastructure.Database;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : DbContext(options), IUnitOfWork
{
    public DbSet<User> Users { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schemas.Swap);
    }
}
