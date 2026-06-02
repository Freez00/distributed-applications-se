using AirlineReservation.Data;
using AirlineReservation.Data.Entities;
using AirlineReservation.Repositories.Interfaces;

namespace AirlineReservation.Repositories.Implementations;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDbContext _context;
    private IRepository<UserEntity>? _users;
    private IRepository<AirportEntity>? _airports;
    private IRepository<AircraftEntity>? _aircraft;
    private IRepository<FlightEntity>? _flights;
    private IRepository<ReservationEntity>? _reservations;
    private IRepository<TicketEntity>? _tickets;
    private IRepository<PaymentEntity>? _payments;

    public UnitOfWork(IDbContext context)
    {
        _context = context;
    }

    public IRepository<UserEntity> Users => _users ??= new Repository<UserEntity>(_context);

    public IRepository<AirportEntity> Airports => _airports ??= new Repository<AirportEntity>(_context);

    public IRepository<AircraftEntity> Aircraft => _aircraft ??= new Repository<AircraftEntity>(_context);

    public IRepository<FlightEntity> Flights => _flights ??= new Repository<FlightEntity>(_context);

    public IRepository<ReservationEntity> Reservations => _reservations ??= new Repository<ReservationEntity>(_context);

    public IRepository<TicketEntity> Tickets => _tickets ??= new Repository<TicketEntity>(_context);

    public IRepository<PaymentEntity> Payments => _payments ??= new Repository<PaymentEntity>(_context);

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);

    public void Dispose()
    {
        if (_context is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
