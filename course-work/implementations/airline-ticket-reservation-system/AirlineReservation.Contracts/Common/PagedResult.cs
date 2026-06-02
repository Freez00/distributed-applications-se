namespace AirlineReservation.Contracts.Common;

public class PagedResult<T>
{
    public IReadOnlyCollection<T> Items { get; set; } = [];

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages { get; set; }
}
