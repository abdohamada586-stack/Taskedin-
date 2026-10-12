
using Microsoft.EntityFrameworkCore;
using Taskedin.Domain.Entities;

namespace Taskedin.Persistence.Context;

public class TaskedinDbContext : DbContext
{
    public TaskedinDbContext(DbContextOptions<TaskedinDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments { get; set; }
    public DbSet<Profile> Profiles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(TaskedinDbContext).Assembly);
    }
}

