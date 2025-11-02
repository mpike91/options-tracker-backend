namespace ProverbsTrading.Models;

public class WeeklyStock
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public DateTime LastUpdated { get; set; }
}
