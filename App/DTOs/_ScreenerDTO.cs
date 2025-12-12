namespace ProverbsTrading.Models.DTOs
{
    public class FilteredOptionDTO
    {
        public double Strike { get; set; }
        public double Bid { get; set; }
        public double Ask { get; set; }
        public double Premium => (Bid + Ask) / 2; // Mid-price for simplicity
        public double Ror => (Premium / Strike) * 100;
        public int Volume { get; set; }
        public int OpenInterest { get; set; }
        public double ImpliedVol { get; set; }
        public double Delta { get; set; } // Include Greeks for advanced analysis
        // Add more as needed, e.g., Gamma, Theta
    }

    public class StockScreenerResultDTO
    {
        public string Symbol { get; set; } = string.Empty;
        public double CurrentPrice { get; set; }
        public double RSI { get; set; }
        public double BBPercent { get; set; }
        public (double SMA50, double SMA100, double SMA200) SMAs { get; set; }
        public List<FilteredOptionDTO> FilteredPuts { get; set; } = new List<FilteredOptionDTO>();
        // Optionally include a snippet of recent history if useful for the frontend chart
    }
}
