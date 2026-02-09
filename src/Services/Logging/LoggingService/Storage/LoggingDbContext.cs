using Microsoft.EntityFrameworkCore;

namespace LoggingService.Storage;

public sealed class LoggingDbContext : DbContext
{
    public LoggingDbContext(DbContextOptions<LoggingDbContext> options) : base(options)
    {
    }

    public DbSet<LogEntry> Logs => Set<LogEntry>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<LogEntry>();

        entity.ToTable("Logs");
        entity.HasKey(log => log.Id);

        entity.Property(log => log.Id)
            .ValueGeneratedNever();

        entity.Property(log => log.TimestampUtc)
            .HasColumnType("datetime2(3)");

        entity.Property(log => log.ServiceName)
            .HasMaxLength(120)
            .IsRequired();

        entity.Property(log => log.Message)
            .HasMaxLength(1024)
            .IsRequired();

        entity.Property(log => log.Severity)
            .HasMaxLength(40)
            .IsRequired();

        entity.Property(log => log.RequestPath)
            .HasMaxLength(400);

        entity.Property(log => log.CorrelationId)
            .HasMaxLength(100);

        entity.Property(log => log.Payload)
            .HasColumnType("nvarchar(max)");

        entity.Property(log => log.StackTrace)
            .HasColumnType("nvarchar(max)");

        entity.HasIndex(log => log.ServiceName);
        entity.HasIndex(log => log.Severity);
        entity.HasIndex(log => log.CorrelationId);
        entity.HasIndex(log => log.RequestPath);
    }
}
