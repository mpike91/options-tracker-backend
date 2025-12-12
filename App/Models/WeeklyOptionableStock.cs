namespace ProverbsTrading.Models;

public class WeeklyOptionableStock
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public WeeklyOptionableStockType Type { get; set; }
    public DateTime LastUpdated { get; set; }
}

public enum WeeklyOptionableStockType
{
    Stock,
    Etf,
}
