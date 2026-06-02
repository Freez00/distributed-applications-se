namespace AirlineReservation.Web.Infrastructure;

public static class QueryStringBuilder
{
    public static string Build(string path, params (string Key, object? Value)[] values)
    {
        var query = values
            .Where(x => x.Value is not null && !string.IsNullOrWhiteSpace(x.Value.ToString()))
            .Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value!.ToString()!)}");

        var queryString = string.Join("&", query);
        return string.IsNullOrWhiteSpace(queryString) ? path : $"{path}?{queryString}";
    }
}
