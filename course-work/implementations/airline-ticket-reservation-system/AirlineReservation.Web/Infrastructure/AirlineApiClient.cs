using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace AirlineReservation.Web.Infrastructure;

public class AirlineApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AirlineApiClient(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task<TResponse> GetAsync<TResponse>(string path, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(HttpMethod.Get, path, body: null, authorize: true, cancellationToken);

    public Task<TResponse> PostAsync<TResponse>(string path, object body, bool authorize = true, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(HttpMethod.Post, path, body, authorize, cancellationToken);

    public Task<TResponse> PutAsync<TResponse>(string path, object body, CancellationToken cancellationToken = default)
        => SendAsync<TResponse>(HttpMethod.Put, path, body, authorize: true, cancellationToken);

    public async Task DeleteAsync(string path, CancellationToken cancellationToken = default)
    {
        using var response = await SendAsync(HttpMethod.Delete, path, body: null, authorize: true, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }
    }

    private async Task<TResponse> SendAsync<TResponse>(HttpMethod method, string path, object? body, bool authorize, CancellationToken cancellationToken)
    {
        using var response = await SendAsync(method, path, body, authorize, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw await CreateApiExceptionAsync(response, cancellationToken);
        }

        var result = await response.Content.ReadFromJsonAsync<TResponse>(JsonOptions, cancellationToken);
        return result ?? throw new InvalidOperationException("The API returned an empty response.");
    }

    private async Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? body, bool authorize, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient("AirlineReservationApi");
        using var request = new HttpRequestMessage(method, path);

        if (body is not null)
        {
            request.Content = JsonContent.Create(body, options: JsonOptions);
        }

        if (authorize)
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString(SessionKeys.AuthToken);
            if (!string.IsNullOrWhiteSpace(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
        }

        return await client.SendAsync(request, cancellationToken);
    }

    private static async Task<ApiException> CreateApiExceptionAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        return new ApiException(response.StatusCode, body);
    }
}
