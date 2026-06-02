using AirlineReservation.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Data;

public interface IDbContext
{
    DbSet<UserEntity> Users { get; }

    DbSet<AirportEntity> Airports { get; }

    DbSet<AircraftEntity> Aircraft { get; }

    DbSet<FlightEntity> Flights { get; }

    DbSet<ReservationEntity> Reservations { get; }

    DbSet<TicketEntity> Tickets { get; }

    DbSet<PaymentEntity> Payments { get; }

    DbSet<TEntity> Set<TEntity>() where TEntity : class;

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
