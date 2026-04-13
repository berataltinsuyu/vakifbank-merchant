using System.Net.Http.Json;

namespace VbMerchant.Services.Concrete;

public class DovizService
{
    private readonly HttpClient _http;

    public DovizService(HttpClient http)
    {
        _http = http;
    }

    // dolar, euro , pound tl cinsinden döner

    public async Task<Dictionary<string, decimal>> KurlariGetirAsync()
    {
        var url = "https://open.er-api.com/v6/latest/TRY";
        var response = await _http.GetFromJsonAsync<ExchangeResponse>(url);

        if (response?.Rates == null)
            return new Dictionary<string, decimal>();

        var istenenler = new[] { "USD", "EUR", "GBP" };

        return response.Rates
            .Where(r => istenenler.Contains(r.Key))
            .ToDictionary(
                r => r.Key,
                r => Math.Round(1 / r.Value, 4)  // tl karşılığına çevirir
            );
    }
}

public record ExchangeResponse(string Result, Dictionary<string, decimal> Rates);