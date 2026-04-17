using System.Text.Json;
using BS.Application.Common;
using BS.Application.Interfaces;

namespace BS.Infrastructure.Services;

internal sealed class OpenLibraryCoverClient : IOpenLibraryCoverClient
{
    private readonly HttpClient _httpClient;

    public OpenLibraryCoverClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string?> GetThumbnailUrl(string isbn, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(isbn))
        {
            return null;
        }

        var clean = IsbnText.Clean(isbn);
        if (clean.Length == 0)
        {
            return null;
        }

        var bibKey = "ISBN:" + clean;
        var path = "api/books?bibkeys=" + Uri.EscapeDataString(bibKey) + "&format=json";
        using var response = await _httpClient.GetAsync(path, cancellationToken).ConfigureAwait(false);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken).ConfigureAwait(false);
        if (!doc.RootElement.TryGetProperty(bibKey, out var book))
        {
            return null;
        }

        if (book.TryGetProperty("thumbnail_url", out var thumb) && thumb.ValueKind == JsonValueKind.String)
        {
            return thumb.GetString();
        }

        if (book.TryGetProperty("cover", out var cover) && cover.TryGetProperty("medium", out var medium) && medium.ValueKind == JsonValueKind.String)
        {
            return medium.GetString();
        }

        return null;
    }
}
