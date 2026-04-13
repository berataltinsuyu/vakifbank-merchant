using System.Net.Http.Json;

namespace VbMerchant.Services.Concrete;

public class GeocodingService
{
    private readonly HttpClient _http;
    private readonly string _apiKey;

    public GeocodingService(HttpClient http, IConfiguration config)
    {
        _http   = http;
        _apiKey = config["GoogleMaps:ApiKey"]!;
    }

    // Adres metnini enlem/boylama çevirir
    // Kullanım: haritadan seçilmeden adres yazılırsa koordinat bulur
    public async Task<(double Enlem, double Boylam)?> AdresCevir(string adres)
    {
        var url = "https://maps.googleapis.com/maps/api/geocode/json" +
                  $"?address={Uri.EscapeDataString(adres)}&key={_apiKey}&language=tr&region=TR";

        var response = await _http.GetFromJsonAsync<GoogleGeoResponse>(url);

        if (response?.Status != "OK" || response.Results.Count == 0)
            return null;

        var loc = response.Results[0].Geometry.Location;
        return (loc.Lat, loc.Lng);
    }
}

// ── Yanıt modelleri ────────────────────────────────────────────────────────
public record GoogleGeoResponse(string Status, List<GeoResult> Results);
public record GeoResult(GeoGeometry Geometry);
public record GeoGeometry(GeoLocation Location);
public record GeoLocation(double Lat, double Lng);