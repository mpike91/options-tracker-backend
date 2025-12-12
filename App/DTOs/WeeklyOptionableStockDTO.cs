namespace ProverbsTrading.Models.DTOs;

public class WeeklyOptionableStockDTO
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}

public class WeeklyOptionableStocksCommaSeparatedDTO
{
    public int Count { get; set; }
    public string CommaSeparatedList { get; set; } = string.Empty;
}
