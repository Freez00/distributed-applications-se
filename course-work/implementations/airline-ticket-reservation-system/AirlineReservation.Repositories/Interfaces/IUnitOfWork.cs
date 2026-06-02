using AirlineReservation.Data.Entities;

namespace AirlineReservation.Repositories.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IRepository<UserEntity> Users { get; }

    IRepository<AirportEntity> Airports { get; }

    IRepository<AircraftEntity> Aircraft { get; }

    IRepository<FlightEntity> Flights { get; }

    IRepository<ReservationEntity> Reservations { get; }

    IRepository<TicketEntity> Tickets { get; }

    IRepository<PaymentEntity> Payments { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
