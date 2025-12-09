using APG.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace APG.Persistence.Data;

/// <summary>
/// Application database context for Entity Framework Core
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    // DbSets
    public DbSet<TestEntity> TestEntities { get; set; }
    public DbSet<BusinessUnit> BusinessUnits { get; set; }
    public DbSet<Sector> Sectors { get; set; }
    public DbSet<Country> Countries { get; set; }
    public DbSet<Currency> Currencies { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<GlobalSalarySettings> GlobalSalarySettings { get; set; }
    public DbSet<ClientMarginSettings> ClientMarginSettings { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply configurations
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Configure TestEntity
        modelBuilder.Entity<TestEntity>(entity =>
        {
            entity.ToTable("TestEntities");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Description)
                .HasMaxLength(1000);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            entity.HasIndex(e => e.Name);
        });

        // Configure BusinessUnit
        modelBuilder.Entity<BusinessUnit>(entity =>
        {
            entity.ToTable("BusinessUnits");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(e => e.ManagerName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on Code
            entity.HasIndex(e => e.Code)
                .IsUnique();
            
            entity.HasIndex(e => e.Name);
            
            // Configure one-to-many relationship with Sectors
            entity.HasMany(e => e.Sectors)
                .WithOne(s => s.BusinessUnit)
                .HasForeignKey(s => s.BusinessUnitId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Configure Sector
        modelBuilder.Entity<Sector>(entity =>
        {
            entity.ToTable("Sectors");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on Name
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Configure Country
        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.IsoCode)
                .HasMaxLength(10);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on Name (case-insensitive)
            entity.HasIndex(e => e.Name)
                .IsUnique();
        });

        // Configure Currency
        modelBuilder.Entity<Currency>(entity =>
        {
            entity.ToTable("Currencies");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(10);
            
            entity.Property(e => e.Symbol)
                .HasMaxLength(10);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on Code
            entity.HasIndex(e => e.Code)
                .IsUnique();
        });

        // Configure Client
        modelBuilder.Entity<Client>(entity =>
        {
            entity.ToTable("Clients");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.DefaultTargetMarginPercent)
                .HasPrecision(5, 2);
            
            entity.Property(e => e.DefaultMinimumMarginPercent)
                .HasPrecision(5, 2);
            
            entity.Property(e => e.DiscountPercent)
                .HasPrecision(5, 2);
            
            entity.Property(e => e.ForcedVacationDaysPerYear);
            
            entity.Property(e => e.ContactName)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.ContactEmail)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on Code (case-insensitive)
            entity.HasIndex(e => e.Code)
                .IsUnique();
            
            entity.HasIndex(e => e.Name);
            
            // Configure relationships
            entity.HasOne(e => e.Sector)
                .WithMany()
                .HasForeignKey(e => e.SectorId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.BusinessUnit)
                .WithMany()
                .HasForeignKey(e => e.BusinessUnitId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Country)
                .WithMany(c => c.Clients)
                .HasForeignKey(e => e.CountryId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.Currency)
                .WithMany(c => c.Clients)
                .HasForeignKey(e => e.CurrencyId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Configure GlobalSalarySettings
        modelBuilder.Entity<GlobalSalarySettings>(entity =>
        {
            entity.ToTable("GlobalSalarySettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.EmployerChargesRate)
                .IsRequired()
                .HasPrecision(5, 2);
            
            entity.Property(e => e.IndirectAnnualCosts)
                .IsRequired()
                .HasPrecision(18, 2);
            
            entity.Property(e => e.BillableHoursPerYear)
                .IsRequired();
            
            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.IsDeleted)
                .IsRequired()
                .HasDefaultValue(false);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Index on IsActive for faster queries
            entity.HasIndex(e => e.IsActive);
            
            // Index on IsDeleted for query filter performance
            entity.HasIndex(e => e.IsDeleted);
            
            // Global query filter to exclude soft-deleted records
            entity.HasQueryFilter(e => !e.IsDeleted);
        });

        // Configure ClientMarginSettings
        modelBuilder.Entity<ClientMarginSettings>(entity =>
        {
            entity.ToTable("ClientMarginSettings");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.ClientId)
                .IsRequired();
            
            entity.Property(e => e.TargetMarginPercent)
                .IsRequired()
                .HasPrecision(5, 2);
            
            entity.Property(e => e.TargetHourlyRate)
                .IsRequired()
                .HasPrecision(18, 2);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on ClientId (one configuration per client)
            entity.HasIndex(e => e.ClientId)
                .IsUnique();
            
            // Configure relationship with Client
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Project
        modelBuilder.Entity<Project>(entity =>
        {
            entity.ToTable("Projects");
            entity.HasKey(e => e.Id);
            
            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Type)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.ResponsibleName)
                .HasMaxLength(200);
            
            entity.Property(e => e.Currency)
                .IsRequired()
                .HasMaxLength(3);
            
            entity.Property(e => e.StartDate)
                .IsRequired();
            
            entity.Property(e => e.EndDate)
                .IsRequired();
            
            entity.Property(e => e.TargetMargin)
                .IsRequired()
                .HasPrecision(5, 2);
            
            entity.Property(e => e.MinMargin)
                .IsRequired()
                .HasPrecision(5, 2);
            
            entity.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);
            
            entity.Property(e => e.Notes)
                .HasMaxLength(2000);
            
            entity.Property(e => e.YtdRevenue)
                .HasPrecision(18, 2);
            
            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");
            
            // Unique constraint on Code (case-insensitive)
            entity.HasIndex(e => e.Code)
                .IsUnique();
            
            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => e.StartDate);
            
            // Configure relationships
            entity.HasOne(e => e.Client)
                .WithMany()
                .HasForeignKey(e => e.ClientId)
                .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasOne(e => e.BusinessUnit)
                .WithMany()
                .HasForeignKey(e => e.BusinessUnitId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Automatically set UpdatedAt for modified entities
        var entries = ChangeTracker.Entries<BaseEntity>()
            .Where(e => e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            entry.Entity.UpdatedAt = DateTime.UtcNow;
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
