namespace ProverbsTrading.Models;

public class OptionExpiration
{
    public int Id { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public DateTime Expiration { get; set; }
    public DateTime LastUpdated { get; set; }
}
