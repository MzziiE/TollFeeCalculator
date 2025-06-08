using System.Text.Json.Serialization;

namespace TollFeeCalculator.Models;

public class HolidaysMapper
{
    [JsonPropertyName("date")]
    public DateTime Date { get; set; }
    [JsonPropertyName("name")]
    public Name Name { get; set; } = new Name();
}

public class Name
{
    [JsonPropertyName("en")]
    public string En { get; set; } = string.Empty;
    [JsonPropertyName("sv")]
    public string Sv { get; set; } = string.Empty;
}