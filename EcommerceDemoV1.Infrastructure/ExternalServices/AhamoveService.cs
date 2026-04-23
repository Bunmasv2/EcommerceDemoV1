using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using EcommerceDemoV1.Domain.Common;
using EcommerceDemoV1.Domain.Entities;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using EcommerceDemoV1.Application.Common;
using System.Threading;
using System.Globalization;

namespace EcommerceDemoV1.Infrastructure.ExternalServices;

public class AhamoveService : IAhamoveService
{
    private readonly HttpClient _http;
    private readonly AhamoveSettings _settings;
    private readonly IMemoryCache _cache;
    private const string TokenCacheKey = "ahamove_token";
    private static readonly SemaphoreSlim _semaphore = new(1, 1);

    public AhamoveService(HttpClient http, IOptions<AhamoveSettings> options, IMemoryCache cache)
    {
        _http = http;
        _settings = options.Value;
        _cache = cache;
    }

    private async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_cache.TryGetValue(TokenCacheKey, out string? cached) && !string.IsNullOrWhiteSpace(cached))
            return cached;

        await _semaphore.WaitAsync(ct);
        try
        {
            //Kiểm tra cache lần 2 (Double-check locking): Nếu Request trước đó đã lấy và lưu vào cache rồi thì lấy ra dùng luôn
            if (_cache.TryGetValue(TokenCacheKey, out cached) && !string.IsNullOrWhiteSpace(cached))
                return cached;

            var apiKey = _settings.ApiKey;
            var phoneShop = _settings.Phone_Shop;

            if (string.IsNullOrWhiteSpace(apiKey))
                throw new InvalidOperationException("Lỗi cấu hình: 'ApiKey' của Ahamove đang trống!");
            if (string.IsNullOrWhiteSpace(phoneShop))
                throw new InvalidOperationException("Lỗi cấu hình: 'Phone_Shop' của Ahamove đang trống!");

            var response = await _http.PostAsJsonAsync($"{_settings.ApiUrl}/accounts/token", new
            {
                api_key = apiKey.Trim(),
                mobile = phoneShop.Trim()
            }, ct);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                throw new InvalidOperationException($"Lỗi cấp quyền Ahamove: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<AhamoveTokenResponse>(cancellationToken: ct)
                ?? throw new InvalidOperationException("Ahamove: Không thể đọc được Token");

            _cache.Set(TokenCacheKey, result.Token, TimeSpan.FromHours(23));
            return result.Token;
        }
        finally
        {
            //Giải phóng luồng
            _semaphore.Release();
        }
    }
    // public async Task<Location> GeocodeAddressAsync(string address)
    // {
    //     await Task.Delay(100);
    //     return new Location(Latitude: 10.7946, Longitude: 106.7216, Address: address);
    // }

    public async Task<Location> GeocodeAddressAsync(string address)
    {
        try
        {
            string locationIqKey = _settings.MapKey;

            // XỬ LÝ CHUỖI: Bỏ các dấu '/' ở số nhà để API dễ tìm hơn (VD: "7/28 Thành Thái" -> "7 28 Thành Thái")
            string cleanAddress = address.Replace("/", " ");

            // API của LocationIQ
            var url = $"https://us1.locationiq.com/v1/search?key={locationIqKey}&q={Uri.EscapeDataString(cleanAddress)}&format=json&limit=1";

            using var request = new HttpRequestMessage(HttpMethod.Get, url);

            var response = await _http.SendAsync(request);
            Console.WriteLine($"[TEST LocationIQ] Request URL: {url}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var results = JsonSerializer.Deserialize<JsonElement>(content, options);

                if (results.ValueKind == JsonValueKind.Array && results.GetArrayLength() > 0)
                {
                    var firstResult = results[0];

                    var latString = firstResult.GetProperty("lat").GetString();
                    var lonString = firstResult.GetProperty("lon").GetString();

                    if (double.TryParse(latString, NumberStyles.Any, CultureInfo.InvariantCulture, out double lat) &&
                        double.TryParse(lonString, NumberStyles.Any, CultureInfo.InvariantCulture, out double lon))
                    {
                        Console.WriteLine($"[TEST LocationIQ] Giao Đến: Lat={lat}, Lng={lon}");
                        return new Location(Latitude: lat, Longitude: lon, Address: address);
                    }
                }
            }
            else
            {
                Console.WriteLine($"[TEST LocationIQ] Failed. Status Code: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GEOCODE ERROR] Lỗi khi dịch địa chỉ: {ex.Message}");
        }

        Console.WriteLine("[GEOCODE WARNING] Không tìm thấy, dùng tọa độ mặc định Landmark 81.");
        return new Location(Latitude: 10.7946, Longitude: 106.7216, Address: address);
    }

    public async Task<EstimateResponse> EstimateOrderFeeAsync(
        Location pickupLocation, Location deliveryLocation, double weightKg, string? serviceId = null)
    {
        var token = await GetTokenAsync();

        Console.WriteLine($"[TEST AHAMOVE] Kho Hàng: Lat={pickupLocation.Latitude}, Lng={pickupLocation.Longitude}");

        Console.WriteLine($"[TEST AHAMOVE] Giao Đến: Lat={deliveryLocation.Latitude}, Lng={deliveryLocation.Longitude}");

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ApiUrl}/orders/estimates");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestBody = new
        {
            order_time = 0,
            payment_method = "BALANCE",
            path = new[]
            {
                new { lat = pickupLocation.Latitude, lng = pickupLocation.Longitude, address = pickupLocation.Address, name = "Kho Hàng", mobile = _settings.Phone_Shop },
                new { lat = deliveryLocation.Latitude, lng = deliveryLocation.Longitude, address = deliveryLocation.Address, name = "Khách Hàng", mobile = "0901234567" }
            },
            services = new[] { new { _id = serviceId ?? _settings.ServiceId, requests = Array.Empty<object>() } }
        };

        req.Content = JsonContent.Create(requestBody);
        var response = await _http.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Lỗi ước tính phí từ Ahamove: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();

        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var resultArray = JsonSerializer.Deserialize<AhamoveV3EstimateResult[]>(responseContent, options);

        if (resultArray == null || !resultArray.Any())
            throw new InvalidOperationException($"Ahamove trả về mảng rỗng. Chi tiết JSON: {responseContent}");

        var firstResult = resultArray.First();

        if (firstResult.Data == null)
            throw new InvalidOperationException($"Ahamove không trả về giá ship (Data = null). {responseContent}");

        return new EstimateResponse(
            ServiceId: firstResult.ServiceId,
            TotalPay: firstResult.Data.TotalPrice,
            Distance: firstResult.Data.Distance,
            EstimatedDuration: firstResult.Data.Duration
        );
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var token = await GetTokenAsync();

        using var req = new HttpRequestMessage(HttpMethod.Post, $"{_settings.ApiUrl}/orders");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var requestBody = new
        {
            order_code = request.OrderCode,
            payment_method = "BALANCE",
            order_time = 0,
            path = new object[]
            {
                new
                {
                    lat = request.PickupLocation.Latitude,
                    lng = request.PickupLocation.Longitude,
                    address = request.PickupLocation.Address,
                    name = "Kho Hàng",
                    mobile = _settings.Phone_Shop,
                    remarks = "Gọi điện trước khi lấy"
                },
                new
                {
                    lat = request.DeliveryLocation.Latitude,
                    lng = request.DeliveryLocation.Longitude,
                    address = request.DeliveryLocation.Address,
                    name = request.DeliveryName,
                    mobile = request.DeliveryPhone,
                    remarks = request.Note,
                    cod = (int)request.CodAmount   // ← tài xế thu hộ nếu COD
                }
            },
            service_id = request.ServiceId ?? _settings.ServiceId,
            requests = Array.Empty<object>()
        };

        req.Content = JsonContent.Create(requestBody);
        var response = await _http.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Lỗi tạo vận đơn Ahamove: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<AhamoveV3OrderResult>()
            ?? throw new InvalidOperationException("Không đọc được response tạo đơn.");

        return new CreateOrderResponse(
            OrderId: result.OrderId,
            status: result.Status,
            ServiceId: result.Order.ServiceId,
            TotalPay: result.Order?.TotalPay ?? 0,
            Distance: result.Order?.Distance ?? 0,
            SharedLink: result.SharedLink ?? ""
        );
    }

    public async Task<OrderStatusResponse> GetOrderStatusAsync(string ahamoveOrderId)
    {
        var token = await GetTokenAsync();

        using var req = new HttpRequestMessage(HttpMethod.Get, $"{_settings.ApiUrl}/orders/{ahamoveOrderId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Lỗi lấy trạng thái vận đơn Ahamove: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent, options);

        var orderId = result.GetProperty("order_id").GetString() ?? ahamoveOrderId;
        var status = result.GetProperty("status").GetString() ?? "IDLE";
        var driverName = result.TryGetProperty("driver_name", out var driverNameElem) ? driverNameElem.GetString() : null;
        var driverPhone = result.TryGetProperty("driver_phone", out var driverPhoneElem) ? driverPhoneElem.GetString() : null;
        var trackingUrl = result.TryGetProperty("shared_link", out var trackingUrlElem) ? trackingUrlElem.GetString() : null;

        return new OrderStatusResponse(orderId, status, driverName, driverPhone, trackingUrl);
    }

    public async Task<IEnumerable<Shipment>> GetShipmentsByAhamoveOrderIdAsync(string ahamoveOrderId)
    {
        var token = await GetTokenAsync();

        using var req = new HttpRequestMessage(HttpMethod.Get, $"{_settings.ApiUrl}/orders/{ahamoveOrderId}");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await _http.SendAsync(req);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"Lỗi lấy Shipment từ Ahamove: {error}");
        }

        var responseContent = await response.Content.ReadAsStringAsync();
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var result = JsonSerializer.Deserialize<JsonElement>(responseContent, options);

        var shipments = new List<Shipment>();

        if (result.TryGetProperty("shipments", out var shipmentsElem) && shipmentsElem.ValueKind == JsonValueKind.Array)
        {
            foreach (var shipmentElem in shipmentsElem.EnumerateArray())
            {
                var shipment = new Shipment
                {
                    AhamoveOrderId = shipmentElem.GetProperty("_id").GetString(),
                    ServiceId = shipmentElem.GetProperty("service_id").GetString(),
                    Status = shipmentElem.GetProperty("status").GetString() ?? "IDLE",
                    TrackingUrl = shipmentElem.TryGetProperty("shared_link", out var linkElem) ? linkElem.GetString() : null,
                    ShippingFee = (decimal)shipmentElem.GetProperty("total_pay").GetDouble(),
                    Distance = shipmentElem.GetProperty("distance").GetDouble()
                };
                shipments.Add(shipment);
            }
        }

        return shipments;
    }
}


file record AhamoveTokenResponse([property: JsonPropertyName("token")] string Token);

file record AhamoveV3EstimateResult(
    [property: JsonPropertyName("service_id")] string ServiceId,
    [property: JsonPropertyName("data")] AhamoveV3EstimateData Data
);

file record AhamoveV3EstimateData(
    [property: JsonPropertyName("distance")] double Distance,
    [property: JsonPropertyName("duration")] int Duration,
    [property: JsonPropertyName("total_price")] decimal TotalPrice
);

// 1. Sửa lại các file record ở cuối file AhamoveService.cs
file record AhamoveV3OrderResult(
    [property: JsonPropertyName("order_id")] string OrderId,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("shared_link")] string? SharedLink,
    [property: JsonPropertyName("order")] AhamoveV3OrderDetail Order
);

// Thêm record này để hứng cục data bên trong
file record AhamoveV3OrderDetail(
    [property: JsonPropertyName("service_id")] string ServiceId,
    [property: JsonPropertyName("total_pay")] decimal TotalPay,
    [property: JsonPropertyName("distance")] double Distance
);