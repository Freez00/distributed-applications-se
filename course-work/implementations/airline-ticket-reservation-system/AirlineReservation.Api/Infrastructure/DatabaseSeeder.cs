using AirlineReservation.ApplicationServices.Implementations;
using AirlineReservation.Data;
using AirlineReservation.Data.Entities;
using AirlineReservation.Data.Enums;
using Microsoft.EntityFrameworkCore;

namespace AirlineReservation.Api.Infrastructure;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AirlineReservationDbContext>();

        await SeedAdminAsync(db);
        await SeedAirportsAsync(db);
        await SeedAircraftAsync(db);
        await SeedFlightsAsync(db);
        await SynchronizeReservationStatusesAsync(db);
        await db.SaveChangesAsync();
    }

    private static async Task SeedAdminAsync(AirlineReservationDbContext db)
    {
        if (await db.Users.AnyAsync(x => x.Email == "admin@airline.local"))
        {
            return;
        }

        db.Users.Add(new UserEntity
        {
            FirstName = "System",
            LastName = "Administrator",
            Email = "admin@airline.local",
            PasswordHash = PasswordHashingService.Hash("Admin123!"),
            Role = UserRole.Admin,
            PhoneNumber = "+359888000000",
            IsActive = true
        });
    }

    private static async Task SeedAirportsAsync(AirlineReservationDbContext db)
    {
        if (await db.Airports.AnyAsync())
        {
            return;
        }

        db.Airports.AddRange(
            new AirportEntity
            {
                Code = "SOF",
                Name = "Sofia Airport",
                City = "Sofia",
                Country = "Bulgaria",
                Timezone = "Europe/Sofia",
                IsActive = true
            },
            new AirportEntity
            {
                Code = "LHR",
                Name = "Heathrow Airport",
                City = "London",
                Country = "United Kingdom",
                Timezone = "Europe/London",
                IsActive = true
            },
            new AirportEntity
            {
                Code = "FCO",
                Name = "Leonardo da Vinci Fiumicino Airport",
                City = "Rome",
                Country = "Italy",
                Timezone = "Europe/Rome",
                IsActive = true
            },
            new AirportEntity
            {
                Code = "CDG",
                Name = "Charles de Gaulle Airport",
                City = "Paris",
                Country = "France",
                Timezone = "Europe/Paris",
                IsActive = true
            });
    }

    private static async Task SeedAircraftAsync(AirlineReservationDbContext db)
    {
        if (await db.Aircraft.AnyAsync())
        {
            return;
        }

        db.Aircraft.AddRange(
            new AircraftEntity
            {
                RegistrationNumber = "LZ-ARB",
                Model = "Airbus A320",
                Manufacturer = "Airbus",
                SeatCapacity = 180,
                RangeKm = 6100,
                ManufactureYear = 2018,
                IsActive = true,
                LastMaintenanceDate = DateTime.UtcNow.AddDays(-21)
            },
            new AircraftEntity
            {
                RegistrationNumber = "LZ-BNG",
                Model = "Boeing 737-800",
                Manufacturer = "Boeing",
                SeatCapacity = 189,
                RangeKm = 5765,
                ManufactureYear = 2017,
                IsActive = true,
                LastMaintenanceDate = DateTime.UtcNow.AddDays(-14)
            });
    }

    private static async Task SeedFlightsAsync(AirlineReservationDbContext db)
    {
        if (await db.Flights.AnyAsync())
        {
            return;
        }

        await db.SaveChangesAsync();

        var sof = await db.Airports.FirstAsync(x => x.Code == "SOF");
        var lhr = await db.Airports.FirstAsync(x => x.Code == "LHR");
        var fco = await db.Airports.FirstAsync(x => x.Code == "FCO");
        var cdg = await db.Airports.FirstAsync(x => x.Code == "CDG");
        var airbus = await db.Aircraft.FirstAsync(x => x.RegistrationNumber == "LZ-ARB");
        var boeing = await db.Aircraft.FirstAsync(x => x.RegistrationNumber == "LZ-BNG");

        db.Flights.AddRange(
            new FlightEntity
            {
                FlightNumber = "FB973",
                DepartureAirportId = sof.Id,
                ArrivalAirportId = fco.Id,
                AircraftId = airbus.Id,
                DepartureTime = DateTime.UtcNow.Date.AddDays(5).AddHours(8),
                ArrivalTime = DateTime.UtcNow.Date.AddDays(5).AddHours(10).AddMinutes(10),
                BasePrice = 119.99m,
                Currency = "EUR",
                Status = FlightStatus.Scheduled
            },
            new FlightEntity
            {
                FlightNumber = "FB851",
                DepartureAirportId = sof.Id,
                ArrivalAirportId = lhr.Id,
                AircraftId = boeing.Id,
                DepartureTime = DateTime.UtcNow.Date.AddDays(7).AddHours(9).AddMinutes(30),
                ArrivalTime = DateTime.UtcNow.Date.AddDays(7).AddHours(12).AddMinutes(45),
                BasePrice = 149.99m,
                Currency = "EUR",
                Status = FlightStatus.Scheduled
            },
            new FlightEntity
            {
                FlightNumber = "FB642",
                DepartureAirportId = cdg.Id,
                ArrivalAirportId = sof.Id,
                AircraftId = airbus.Id,
                DepartureTime = DateTime.UtcNow.Date.AddDays(10).AddHours(13),
                ArrivalTime = DateTime.UtcNow.Date.AddDays(10).AddHours(15).AddMinutes(40),
                BasePrice = 132.50m,
                Currency = "EUR",
                Status = FlightStatus.Scheduled
            });
    }

    private static async Task SynchronizeReservationStatusesAsync(AirlineReservationDbContext db)
    {
        var reservations = await db.Reservations
            .Include(x => x.Payments)
            .Include(x => x.Tickets)
            .Where(x =>
                x.Payments.Any(payment =>
                    payment.PaymentStatus == PaymentStatus.Completed ||
                    payment.PaymentStatus == PaymentStatus.Refunded))
            .ToListAsync();

        foreach (var reservation in reservations)
        {
            if (reservation.Payments.Any(payment => payment.PaymentStatus == PaymentStatus.Completed) &&
                reservation.Status == ReservationStatus.Pending)
            {
                reservation.Status = ReservationStatus.Confirmed;
                reservation.ExpiresAt = null;
            }
            else if (reservation.Payments.All(payment => payment.PaymentStatus != PaymentStatus.Completed) &&
                reservation.Payments.Any(payment => payment.PaymentStatus == PaymentStatus.Refunded) &&
                reservation.Status != ReservationStatus.Cancelled)
            {
                reservation.Status = ReservationStatus.Cancelled;
                reservation.ExpiresAt = null;
            }

            if (reservation.Payments.Any(payment => payment.PaymentStatus == PaymentStatus.Refunded))
            {
                foreach (var ticket in reservation.Tickets.Where(x => x.TicketStatus is TicketStatus.Issued or TicketStatus.CheckedIn))
                {
                    ticket.TicketStatus = TicketStatus.Refunded;
                }
            }
        }
    }
}
