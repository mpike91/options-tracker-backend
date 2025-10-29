namespace ProverbsTrading.Models.DTOs;

public class WeeklyStockDTO
{
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}