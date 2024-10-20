using Microsoft.EntityFrameworkCore;
using Uttom.Domain.Messages;
using Uttom.Domain.Models;
using Uttom.Infrastructure.Configurations;

namespace Uttom.Infrastructure.Implementations;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Motorcycle> Motorcycles { get; set; }
    public DbSet<RegisteredMotorcycle> RegisteredMotorcycles { get; set; }
    public DbSet<Deliverer> Deliverers { get; set; }
    public DbSet<Rental> Rentals { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new TrackableEntityConfiguration<Motorcycle>());
        modelBuilder.ApplyConfiguration(new TrackableEntityConfiguration<Deliverer>());
        modelBuilder.ApplyConfiguration(new TrackableEntityConfiguration<Rental>());

        // -- Motorcycle
        modelBuilder.Entity<Motorcycle>()
            .ToTable("Motorcycles")
            .HasKey(x => x.Id);

        modelBuilder.Entity<Motorcycle>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Motorcycle>()
            .HasIndex(m => m.PlateNumber)
            .IsUnique();

        // -- RegisteredMotorcycle
        modelBuilder.Entity<RegisteredMotorcycle>()
            .ToTable("RegisteredMotorcycles")
            .Property(m => m.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<RegisteredMotorcycle>()
            .Property(m => m.Identifier)
            .IsRequired();

        modelBuilder.Entity<RegisteredMotorcycle>()
            .Property(m => m.Year)
            .IsRequired();

        modelBuilder.Entity<RegisteredMotorcycle>()
            .Property(m => m.Model)
            .IsRequired();

        modelBuilder.Entity<RegisteredMotorcycle>()
            .Property(m => m.PlateNumber)
            .IsRequired();

        // -- Deliverer
        modelBuilder.Entity<Deliverer>()
            .ToTable("Deliverers")
            .HasKey(x => x.Id);

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Deliverer>()
            .HasIndex(x => x.BusinessTaxId)
            .IsUnique();

        modelBuilder.Entity<Deliverer>()
            .HasIndex(x => x.DriverLicenseNumber)
            .IsUnique();

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.Identifier)
            .HasColumnType("varchar(50)")
            .IsRequired();

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.Name)
            .HasColumnType("varchar(50)")
            .IsRequired();

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.BusinessTaxId)
            .HasColumnType("varchar(50)")
            .IsRequired();

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.DateOfBirth)
            .HasColumnType("date")
            .IsRequired();

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.DriverLicenseNumber)
            .HasColumnType("varchar(50)")
            .IsRequired();

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.DriverLicenseImageId)
            .HasColumnType("varchar(50)");

        modelBuilder.Entity<Deliverer>()
            .Property(x => x.DriverLicenseType)
            .HasConversion<string>()
            .IsRequired();

        // Rental
        modelBuilder.Entity<Rental>()
            .ToTable("Rentals")
            .HasKey(x => x.Id);

        modelBuilder.Entity<Rental>()
            .Property(x => x.Id)
            .ValueGeneratedOnAdd();

        modelBuilder.Entity<Rental>()
            .Property(x => x.PlanId)
            .HasColumnType("int")
            .IsRequired();

        modelBuilder.Entity<Rental>()
            .Property(x => x.StartDate)
            .HasColumnType("date")
            .IsRequired();

        modelBuilder.Entity<Rental>()
            .Property(x => x.EndDate)
            .HasColumnType("date")
            .IsRequired();

        modelBuilder.Entity<Rental>()
            .Property(x => x.EstimatingEndingDate)
            .HasColumnType("date")
            .IsRequired();

        modelBuilder.Entity<Rental>()
            .Property(x => x.ReturnDate)
            .HasColumnType("date");

        modelBuilder.Entity<Rental>()
            .Property(x => x.DelivererId)
            .HasColumnType("int")
            .IsRequired();

        modelBuilder.Entity<Rental>()
            .HasOne<Deliverer>(x => x.Deliverer)
            .WithMany()
            .HasForeignKey(x => x.DelivererId);

        modelBuilder.Entity<Rental>()
            .HasOne<Motorcycle>(x => x.Motorcycle)
            .WithMany()
            .HasForeignKey(x => x.MotorcycleId);
    }
}