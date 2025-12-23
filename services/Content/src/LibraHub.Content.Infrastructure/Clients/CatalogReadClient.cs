using LibraHub.Content.Application.Abstractions;
using LibraHub.Content.Application.Options;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace LibraHub.Content.Infrastructure.Clients;

public class CatalogReadClient : ICatalogReadClient
{
    private readonly HttpClient _httpClient;
    private readonly ReadAccessOptions _options;
    private readonly JsonSerializerOptions _jsonOptions;

    public CatalogReadClient(HttpClient httpClient, IOptions<ReadAccessOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BookInfo?> GetBookInfoAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"{_options.CatalogApiUrl}/api/books/{bookId}/info",
                cancellationToken);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var bookInfo = JsonSerializer.Deserialize<BookInfo>(content, _jsonOptions);

            return bookInfo;
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }
}

