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

    public static void SeedData(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        if (!context.Resources.Any())
        {
            context.Resources.AddRange(
                new Resource { Name = "Meeting Room Alpha", Description = "Large room with projector and whiteboard", Location = "3rd Floor, West Wing", Capacity = 10, IsAvailable = true },
                new Resource { Name = "Company Car 1", Description = "Compact sedan", Location = "Parking Bay 5", Capacity = 5, IsAvailable = true },
                new Resource { Name = "Company Car 2", Description = "VW Polo TSI", Location = "Parking Bay 6", Capacity = 5, IsAvailable = true },
                new Resource { Name = "Company Car 3", Description = "VW R-Line GTI", Location = "Parking Bay 7", Capacity = 5, IsAvailable = true },
                new Resource { Name = "Company Car 4", Description = "VW T-cross R-Line", Location = "Parking Bay 8", Capacity = 5, IsAvailable = true },
                new Resource { Name = "Company Car 5", Description = "Renault Clio", Location = "Parking Bay 9", Capacity = 5, IsAvailable = true },
                new Resource { Name = "Conference Room Beta", Description = "Medium room with video conferencing", Location = "2nd Floor, East Wing", Capacity = 8, IsAvailable = true },
                new Resource { Name = "Projector", Description = "HD projector", Location = "Equipment Room", Capacity = 1, IsAvailable = true },
                new Resource { Name = "Projector 2", Description = "HD projector", Location = "Equipment Room", Capacity = 1, IsAvailable = true },
                new Resource { Name = "Projector 3", Description = "HD projector", Location = "Equipment Room", Capacity = 1, IsAvailable = true },
                new Resource { Name = "Projector 4", Description = "HD projector", Location = "Equipment Room", Capacity = 1, IsAvailable = true },
                new Resource { Name = "Projector 5", Description = "HD projector", Location = "Equipment Room", Capacity = 1, IsAvailable = true }
            );
            context.SaveChanges();
        }
    }
} 