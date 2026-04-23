namespace EcommerceDemoV1.Application.Common;

public class AhamoveSettings
{
    public string ApiUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string Address { get; set; } = string.Empty;
    public string ServiceId { get; set; } = "SGN-BIKE";
    public string Phone_Shop { get; set; } = string.Empty;
    public string MapKey { get; set; } = string.Empty;
}