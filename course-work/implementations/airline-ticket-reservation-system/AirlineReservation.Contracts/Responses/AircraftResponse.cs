namespace AirlineReservation.Contracts.Responses;

public class AircraftResponse
{
    public int Id { get; set; }

    public string RegistrationNumber { get; set; } = string.Empty;

    public string Model { get; set; } = string.Empty;

    public string Manufacturer { get; set; } = string.Empty;

    public int SeatCapacity { get; set; }

    public int RangeKm { get; set; }

    public int ManufactureYear { get; set; }

    public bool IsActive { get; set; }

    public DateTime? LastMaintenanceDate { get; set; }

    public DateTime CreatedAt { get; set; }
}
