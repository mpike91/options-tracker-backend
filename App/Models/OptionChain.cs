namespace ProverbsTrading.Models;

public class OptionChain {
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public string OptionType { get; set; } = string.Empty;  // "call" or "put"
    public double Strike { get; set; }
    public double Bid { get; set; }
    public double Ask { get; set; }
    public int Volume { get; set; }
    public int OpenInterest { get; set; }
    public double ImpliedVol { get; set; }
    public double Delta { get; set; }
    public double Gamma { get; set; }
    public double Theta { get; set; }
    public double Vega { get; set; }
    public double Rho { get; set; }
    public DateTime LastUpdated { get; set; }
}