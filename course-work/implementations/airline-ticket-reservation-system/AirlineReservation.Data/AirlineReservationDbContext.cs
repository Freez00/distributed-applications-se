using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Data;

public class AirlineReservationDbContext : DbContext, IDbContext
{
    public AirlineReservationDbContext(DbContextOptions<AirlineReservationDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserEntity> Users => Set<UserEntity>();

    public DbSet<AirportEntity> Airports => Set<AirportEntity>();

    public DbSet<AircraftEntity> Aircraft => Set<AircraftEntity>();

    public DbSet<FlightEntity> Flights => Set<FlightEntity>();

    public DbSet<ReservationEntity> Reservations => Set<ReservationEntity>();

    public DbSet<TicketEntity> Tickets => Set<TicketEntity>();

    public DbSet<PaymentEntity> Payments => Set<PaymentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsers(modelBuilder);
        ConfigureAirports(modelBuilder);
        ConfigureAircraft(modelBuilder);
        ConfigureFlights(modelBuilder);
        ConfigureReservations(modelBuilder);
        ConfigureTickets(modelBuilder);
        ConfigurePayments(modelBuilder);
    }

    private static void ConfigureUsers(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.HasIndex(x => x.Email).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.Role).HasConversion<string>().HasMaxLength(20);
        });
    }

    private static void ConfigureAirports(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AirportEntity>(entity =>
        {
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private static void ConfigureAircraft(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AircraftEntity>(entity =>
        {
            entity.HasIndex(x => x.RegistrationNumber).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
        });
    }

    private static void ConfigureFlights(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FlightEntity>(entity =>
        {
            entity.HasIndex(x => x.FlightNumber).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.BasePrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(x => x.DepartureAirport)
                .WithMany(x => x.DepartingFlights)
                .HasForeignKey(x => x.DepartureAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.ArrivalAirport)
                .WithMany(x => x.ArrivingFlights)
                .HasForeignKey(x => x.ArrivalAirportId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Aircraft)
                .WithMany(x => x.Flights)
                .HasForeignKey(x => x.AircraftId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureReservations(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReservationEntity>(entity =>
        {
            entity.HasIndex(x => x.ReservationCode).IsUnique();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.BookingDate).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(x => x.User)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Flight)
                .WithMany(x => x.Reservations)
                .HasForeignKey(x => x.FlightId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    private static void ConfigureTickets(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TicketEntity>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.PassengerType).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.FareClass).HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.TicketStatus).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(x => x.Reservation)
                .WithMany(x => x.Tickets)
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(x => new { x.ReservationId, x.PassengerDocumentNumber });
        });
    }

    private static void ConfigurePayments(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PaymentEntity>(entity =>
        {
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");
            entity.Property(x => x.Amount).HasColumnType("decimal(18,2)");
            entity.Property(x => x.PaymentMethod).HasConversion<string>().HasMaxLength(30);
            entity.Property(x => x.PaymentStatus).HasConversion<string>().HasMaxLength(30);

            entity.HasOne(x => x.Reservation)
                .WithMany(x => x.Payments)
                .HasForeignKey(x => x.ReservationId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
