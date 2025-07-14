using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Resource> Resources { get; set; }
    public DbSet<Booking> Bookings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Resource>()
            .HasMany(r => r.Bookings)
            .WithOne(b => b.Resource)
            .HasForeignKey(b => b.ResourceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
} 