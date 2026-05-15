using System.Text.Json;

namespace STranslate.Plugin.Dictionary.ECDICT.Anki;

public class AnkiConnectClient
{
    private readonly string _baseUrl;
    private readonly IHttpService _httpService;

    public AnkiConnectClient(string baseUrl, IHttpService httpService)
    {
        _baseUrl = baseUrl.TrimEnd('/');
        _httpService = httpService;
    }

    public async Task<T> InvokeAsync<T>(string action, object? parameters = null, CancellationToken cancellationToken = default)
    {
        object request = parameters is null
            ? new { action, version = 6 }
            : new { action, version = 6, @params = parameters };

        var options = new Options { ContentType = "application/json" };
        var response = await _httpService.PostAsync(_baseUrl, request, options, cancellationToken);

        using var document = JsonDocument.Parse(response);

        if (document.RootElement.TryGetProperty("error", out var error) &&
            error.ValueKind != JsonValueKind.Null)
        {
            throw new AnkiConnectException(error.GetString() ?? "Unknown AnkiConnect error");
        }

        if (!document.RootElement.TryGetProperty("result", out var result))
        {
            throw new AnkiConnectException("No result in AnkiConnect response");
        }

        if (result.ValueKind == JsonValueKind.Null)
        {
            return default!;
        }

        var resultJson = result.GetRawText();
        return JsonSerializer.Deserialize<T>(resultJson)!;
    }

    public async Task<bool> CheckConnectionAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await InvokeAsync<int>("version", null, cancellationToken);
            return version > 0;
        }
        catch
        {
            return false;
        }
    }
}
