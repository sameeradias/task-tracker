using Microsoft.EntityFrameworkCore;

namespace backend.Data;

/// <summary>
/// Application database context with NoTracking by default for optimal performance.
/// </summary>
public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    // Future entity sets - uncomment and configure when entities are created
    // public DbSet<Task> Tasks => Set<Task>();
    // public DbSet<User> Users => Set<User>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // Set NoTracking as default behavior for better performance
        // Use AsTracking() explicitly when change tracking is needed
        optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Future entity configurations will be added here
        // modelBuilder.ApplyConfiguration(new TaskEntityConfiguration());
        // modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
    }
}