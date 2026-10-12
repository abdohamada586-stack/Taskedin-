using Microsoft.EntityFrameworkCore;
using Taskedin.Domain.Entities;

namespace Taskedin.Persistence;

public class TaskedinDbContext : DbContext
{
    public TaskedinDbContext(DbContextOptions<TaskedinDbContext> options)
        : base(options)
    {
    }

    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Profile> Profiles => Set<Profile>();
}
