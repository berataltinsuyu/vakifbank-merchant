using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;

namespace VbMerchant.Services;

public sealed class SupabaseStorageService : ISupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly SupabaseStorageOptions _options;

    public SupabaseStorageService(HttpClient httpClient, IOptions<SupabaseStorageOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public long MaxFileSizeBytes => _options.MaxFileSizeBytes > 0
        ? _options.MaxFileSizeBytes
        : 5 * 1024 * 1024;

    public async Task UploadAsync(
        string objectPath,
        Stream content,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildObjectUrl(objectPath));
        ApplyAuthHeaders(request);

        request.Headers.CacheControl = new CacheControlHeaderValue
        {
            MaxAge = TimeSpan.FromSeconds(3600)
        };

        request.Content = new StreamContent(content);
        request.Content.Headers.ContentType = new MediaTypeHeaderValue(contentType);

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new InvalidOperationException(
                $"Supabase Storage upload failed. Status={(int)response.StatusCode}. Body={body}");
        }
    }

    public async Task<string?> CreateSignedUrlAsync(
        string objectPath,
        CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, BuildSignUrl(objectPath));
        ApplyAuthHeaders(request);

        request.Content = new StringContent(
            JsonSerializer.Serialize(new { expiresIn = GetSignedUrlExpiresSeconds() }),
            Encoding.UTF8,
            "application/json");

        using var response = await _httpClient.SendAsync(request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var json = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
        if (!json.RootElement.TryGetProperty("signedURL", out var signedUrlElement))
        {
            return null;
        }

        var signedUrl = signedUrlElement.GetString();
        if (string.IsNullOrWhiteSpace(signedUrl))
        {
            return null;
        }

        return signedUrl.StartsWith("http", StringComparison.OrdinalIgnoreCase)
            ? signedUrl
            : $"{GetStorageBaseUrl()}{signedUrl}";
    }

    private Uri BuildObjectUrl(string objectPath)
    {
        return new Uri($"{GetStorageBaseUrl()}/object/{GetBucket()}/{EscapeObjectPath(objectPath)}");
    }

    private Uri BuildSignUrl(string objectPath)
    {
        return new Uri($"{GetStorageBaseUrl()}/object/sign/{GetBucket()}/{EscapeObjectPath(objectPath)}");
    }

    private void ApplyAuthHeaders(HttpRequestMessage request)
    {
        var serviceRoleKey = GetServiceRoleKey();
        request.Headers.TryAddWithoutValidation("apikey", serviceRoleKey);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", serviceRoleKey);
    }

    private string GetStorageBaseUrl()
    {
        if (string.IsNullOrWhiteSpace(_options.Url))
        {
            throw new InvalidOperationException("Supabase:Url yapılandırması eksik.");
        }

        return $"{_options.Url.TrimEnd('/')}/storage/v1";
    }

    private string GetServiceRoleKey()
    {
        if (string.IsNullOrWhiteSpace(_options.ServiceRoleKey))
        {
            throw new InvalidOperationException("Supabase:ServiceRoleKey yapılandırması eksik.");
        }

        return _options.ServiceRoleKey;
    }

    private string GetBucket()
    {
        if (string.IsNullOrWhiteSpace(_options.StorageBucket))
        {
            throw new InvalidOperationException("Supabase:StorageBucket yapılandırması eksik.");
        }

        return Uri.EscapeDataString(_options.StorageBucket);
    }

    private int GetSignedUrlExpiresSeconds()
    {
        return _options.SignedUrlExpiresSeconds > 0
            ? _options.SignedUrlExpiresSeconds
            : 300;
    }

    private static string EscapeObjectPath(string objectPath)
    {
        return string.Join(
            '/',
            objectPath
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));
    }
}
