namespace ProverbsTrading.Models;

public class MarketHistory {
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public double Open { get; set; }
    public double High { get; set; }
    public double Low { get; set; }
    public double Close { get; set; }
    public int Volume { get; set; }
    public DateTime LastUpdated { get; set; }
}