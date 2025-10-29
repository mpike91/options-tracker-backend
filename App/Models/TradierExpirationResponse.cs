using System.Text.Json.Serialization;

namespace ProverbsTrading.Models;

public class TradierExpirationResponse {
    [JsonPropertyName("expirations")]
    public List<string> Expirations { get; set; } = new List<string>();
}