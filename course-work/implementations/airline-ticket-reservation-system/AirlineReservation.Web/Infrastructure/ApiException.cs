using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace AirlineReservation.Web.Infrastructure;

public class ApiException : Exception
{
    public ApiException(HttpStatusCode statusCode, string responseBody)
        : base($"API request failed with status {(int)statusCode}.")
    {
        StatusCode = statusCode;
        ResponseBody = responseBody;
        FriendlyMessages = BuildFriendlyMessages(statusCode, responseBody);
    }

    public HttpStatusCode StatusCode { get; }

    public string ResponseBody { get; }

    public IReadOnlyCollection<string> FriendlyMessages { get; }

    public string FriendlyMessage => string.Join(Environment.NewLine, FriendlyMessages);

    private static IReadOnlyCollection<string> BuildFriendlyMessages(HttpStatusCode statusCode, string responseBody)
    {
        if (string.IsNullOrWhiteSpace(responseBody))
        {
            return [DefaultMessage(statusCode)];
        }

        try
        {
            using var document = JsonDocument.Parse(responseBody);
            var root = document.RootElement;
            var messages = new List<string>();

            if (root.TryGetProperty("errors", out var errors) && errors.ValueKind == JsonValueKind.Object)
            {
                foreach (var errorProperty in errors.EnumerateObject())
                {
                    var fieldName = FormatFieldName(errorProperty.Name);
                    if (errorProperty.Value.ValueKind != JsonValueKind.Array)
                    {
                        continue;
                    }

                    foreach (var error in errorProperty.Value.EnumerateArray())
                    {
                        if (error.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(error.GetString()))
                        {
                            messages.Add(CleanValidationMessage(error.GetString()!, errorProperty.Name, fieldName));
                        }
                    }
                }
            }

            if (messages.Count == 0 && root.TryGetProperty("detail", out var detail) && detail.ValueKind == JsonValueKind.String)
            {
                AddIfUseful(messages, detail.GetString());
            }

            if (messages.Count == 0 && root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
            {
                AddIfUseful(messages, title.GetString());
            }

            return messages.Count == 0
                ? [DefaultMessage(statusCode)]
                : messages.Distinct(StringComparer.OrdinalIgnoreCase).ToList();
        }
        catch (JsonException)
        {
            return LooksLikeRawPayload(responseBody)
                ? [DefaultMessage(statusCode)]
                : [responseBody.Trim()];
        }
    }

    private static void AddIfUseful(ICollection<string> messages, string? message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        var trimmed = message.Trim();
        if (!trimmed.Equals("One or more validation errors occurred.", StringComparison.OrdinalIgnoreCase))
        {
            messages.Add(EnsureSentence(trimmed));
        }
    }

    private static string CleanValidationMessage(string message, string rawFieldName, string displayFieldName)
    {
        var minimumLength = Regex.Match(message, @"minimum length of '(\d+)'", RegexOptions.IgnoreCase);
        if (minimumLength.Success)
        {
            return $"{displayFieldName} must be at least {minimumLength.Groups[1].Value} characters.";
        }

        var maximumLength = Regex.Match(message, @"maximum length of '(\d+)'", RegexOptions.IgnoreCase);
        if (maximumLength.Success)
        {
            return $"{displayFieldName} must be at most {maximumLength.Groups[1].Value} characters.";
        }

        var range = Regex.Match(message, @"between ([^ ]+) and ([^ .]+)", RegexOptions.IgnoreCase);
        if (range.Success)
        {
            return $"{displayFieldName} must be between {range.Groups[1].Value} and {range.Groups[2].Value}.";
        }

        if (message.Contains("field is required", StringComparison.OrdinalIgnoreCase) ||
            message.Contains("is required", StringComparison.OrdinalIgnoreCase))
        {
            return $"{displayFieldName} is required.";
        }

        var cleaned = message
            .Replace($"The field {rawFieldName}", displayFieldName, StringComparison.OrdinalIgnoreCase)
            .Replace($"The {rawFieldName} field", displayFieldName, StringComparison.OrdinalIgnoreCase)
            .Replace(rawFieldName, displayFieldName, StringComparison.OrdinalIgnoreCase);

        var lastSegment = rawFieldName.Split('.').Last();
        cleaned = cleaned
            .Replace($"The field {lastSegment}", displayFieldName, StringComparison.OrdinalIgnoreCase)
            .Replace($"The {lastSegment} field", displayFieldName, StringComparison.OrdinalIgnoreCase)
            .Replace(lastSegment, displayFieldName, StringComparison.OrdinalIgnoreCase);

        return EnsureSentence(cleaned);
    }

    private static string FormatFieldName(string fieldName)
    {
        var ticketMatch = Regex.Match(fieldName, @"^Tickets\[(\d+)\]\.(.+)$", RegexOptions.IgnoreCase);
        if (ticketMatch.Success)
        {
            var passengerNumber = int.Parse(ticketMatch.Groups[1].Value) + 1;
            return $"Passenger {passengerNumber} {FormatPropertyName(ticketMatch.Groups[2].Value, removePassengerPrefix: true)}";
        }

        return FormatPropertyName(fieldName.Split('.').Last(), removePassengerPrefix: false);
    }

    private static string FormatPropertyName(string propertyName, bool removePassengerPrefix)
    {
        if (removePassengerPrefix && propertyName.StartsWith("Passenger", StringComparison.Ordinal))
        {
            propertyName = propertyName["Passenger".Length..];
        }

        return Regex.Replace(propertyName, "([a-z0-9])([A-Z])", "$1 $2").ToLowerInvariant();
    }

    private static bool LooksLikeRawPayload(string responseBody)
    {
        var trimmed = responseBody.TrimStart();
        return trimmed.StartsWith('{') || trimmed.StartsWith('[') || trimmed.StartsWith('<');
    }

    private static string EnsureSentence(string message)
    {
        var trimmed = message.Trim();
        return trimmed.EndsWith('.') ? trimmed : $"{trimmed}.";
    }

    private static string DefaultMessage(HttpStatusCode statusCode)
        => statusCode switch
        {
            HttpStatusCode.BadRequest => "The request contains invalid data. Please review the highlighted fields.",
            HttpStatusCode.Unauthorized => "Please sign in and try again.",
            HttpStatusCode.Forbidden => "You do not have permission to perform this action.",
            HttpStatusCode.NotFound => "The requested record was not found.",
            HttpStatusCode.Conflict => "The request could not be completed because it conflicts with the current data.",
            _ => "The request could not be completed. Please try again."
        };
}
