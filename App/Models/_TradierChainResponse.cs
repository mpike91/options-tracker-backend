using System.Text.Json.Serialization;

namespace ProverbsTrading.Models;

public class TradierChainResponse
{
    [JsonPropertyName("options")]
    public List<TradierOption> Options { get; set; } = new List<TradierOption>();
}

public class TradierOption
{
    [JsonPropertyName("option_type")]
    public string? OptionType { get; set; }

    [JsonPropertyName("strike")]
    public double Strike { get; set; }

    [JsonPropertyName("bid")]
    public double Bid { get; set; }

    [JsonPropertyName("ask")]
    public double Ask { get; set; }

    [JsonPropertyName("volume")]
    public int Volume { get; set; }

    [JsonPropertyName("open_interest")]
    public int OpenInterest { get; set; }

    [JsonPropertyName("implied_volatility")]
    public double ImpliedVol { get; set; }

    [JsonPropertyName("greeks")]
    public TradierGreeks? Greeks { get; set; }
}

public class TradierGreeks
{
    [JsonPropertyName("delta")]
    public double Delta { get; set; }

    [JsonPropertyName("gamma")]
    public double Gamma { get; set; }

    [JsonPropertyName("theta")]
    public double Theta { get; set; }

    [JsonPropertyName("vega")]
    public double Vega { get; set; }

    [JsonPropertyName("rho")]
    public double Rho { get; set; }
}
