
using Microsoft.EntityFrameworkCore;
using Taskedin.Domain.Entities;

namespace Taskedin.Persistence.Context;

// EF Core context for the Taskedin domain; all entity mappings live in Configurations.
public class TaskedinDbContext : DbContext
{
    public TaskedinDbContext(DbContextOptions<TaskedinDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<Role> Roles { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TaskedinDbContext).Assembly);
    }
}

