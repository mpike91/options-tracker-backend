// using System.TimeZoneInfo;
using System.Linq;
using System.Net.Http.Headers;
using System.Text.Json;
using MathNet.Numerics.Statistics;
using Microsoft.EntityFrameworkCore;
using ProverbsTrading.Models;
using ProverbsTrading.Models.DTOs;

public class TradierService
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _config;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(30);

    public TradierService(AppDbContext db, IConfiguration config)
    {
        _db = db;
        _config = config;
        _httpClient = new HttpClient { BaseAddress = new Uri("https://api.tradier.com/v1/") };
        _httpClient.DefaultRequestHeaders.Add(
            "Authorization",
            $"Bearer {_config["TradierApiKey"]}"
        );
        _httpClient.DefaultRequestHeaders.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json")
        );
    }

    public async Task FetchExpirationsForAllStocksAsync()
    {
        var stocks = await _db.WeeklyStocks.Select(s => s.Symbol).ToListAsync();
        var tasks = new List<Task>();
        foreach (var symbol in stocks)
        {
            await _semaphore.WaitAsync();
            tasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(
                            $"markets/options/expirations?symbol={symbol}"
                        );
                        response.EnsureSuccessStatusCode();
                        var json = await response.Content.ReadAsStringAsync();
                        var expirationsResponse =
                            JsonSerializer.Deserialize<TradierExpirationResponse>(json);

                        var expirationsList =
                            expirationsResponse != null
                                ? expirationsResponse
                                    .Expirations.Select(e => DateTime.Parse(e!))
                                    .ToList()
                                : new List<DateTime>();

                        var nextExpirations = expirationsList
                            .Where(e => e > DateTime.Today && e.DayOfWeek == DayOfWeek.Friday)
                            .OrderBy(e => e)
                            .Take(2)
                            .ToList();
                        _db.OptionExpirations.RemoveRange(
                            _db.OptionExpirations.Where(o => o.Symbol == symbol)
                        );
                        _db.OptionExpirations.AddRange(
                            nextExpirations.Select(e => new OptionExpiration
                            {
                                Symbol = symbol,
                                Expiration = e,
                                LastUpdated = DateTimeUtils.GetEasternTime(),
                            })
                        );
                        await _db.SaveChangesAsync();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                })
            );
        }
        await Task.WhenAll(tasks);
    }

    public async Task FetchChainsForAllStocksAsync()
    {
        var expirations = await _db.OptionExpirations.ToListAsync();
        var tasks = new List<Task>();
        foreach (var exp in expirations)
        {
            await _semaphore.WaitAsync();
            tasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(
                            $"markets/options/chains?symbol={exp.Symbol}&expiration={exp.Expiration:yyyy-MM-dd}&greeks=true"
                        );
                        response.EnsureSuccessStatusCode();
                        var json = await response.Content.ReadAsStringAsync();
                        var chainResponse = JsonSerializer.Deserialize<TradierChainResponse>(json);

                        var chains =
                            chainResponse != null
                                ? chainResponse
                                    .Options.Select(c => new OptionChain
                                    {
                                        Symbol = exp.Symbol,
                                        Expiration = exp.Expiration,
                                        OptionType = c.OptionType ?? "",
                                        Strike = c.Strike,
                                        Bid = c.Bid,
                                        Ask = c.Ask,
                                        Volume = c.Volume,
                                        OpenInterest = c.OpenInterest,
                                        ImpliedVol = c.ImpliedVol,
                                        Delta = c.Greeks?.Delta ?? 0,
                                        Gamma = c.Greeks?.Gamma ?? 0,
                                        Theta = c.Greeks?.Theta ?? 0,
                                        Vega = c.Greeks?.Vega ?? 0,
                                        Rho = c.Greeks?.Rho ?? 0,
                                        LastUpdated = DateTimeUtils.GetEasternTime(),
                                    })
                                    .ToList()
                                : new List<OptionChain>();

                        _db.OptionChains.RemoveRange(
                            _db.OptionChains.Where(o =>
                                o.Symbol == exp.Symbol && o.Expiration == exp.Expiration
                            )
                        );
                        _db.OptionChains.AddRange(chains);
                        await _db.SaveChangesAsync();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                })
            );
        }
        await Task.WhenAll(tasks);
    }

    public async Task FetchHistoryForAllStocksAsync()
    {
        var stocks = await _db.WeeklyStocks.Select(s => s.Symbol).ToListAsync();
        var tasks = new List<Task>();
        var startDate = DateTime.Today.AddYears(-1).ToString("yyyy-MM-dd");
        var endDate = DateTime.Today.ToString("yyyy-MM-dd");
        foreach (var symbol in stocks)
        {
            await _semaphore.WaitAsync();
            tasks.Add(
                Task.Run(async () =>
                {
                    try
                    {
                        var response = await _httpClient.GetAsync(
                            $"markets/history?symbol={symbol}&interval=1d&start={startDate}&end={endDate}"
                        );
                        response.EnsureSuccessStatusCode();
                        var json = await response.Content.ReadAsStringAsync();
                        var historyResponse = JsonSerializer.Deserialize<TradierHistoryResponse>(
                            json
                        );

                        var history =
                            historyResponse != null
                                ? historyResponse
                                    .History.Select(h => new MarketHistory
                                    {
                                        Symbol = symbol,
                                        Date = DateTime.Parse(h.Date ?? "1900-01-01"),
                                        Open = h.Open,
                                        High = h.High,
                                        Low = h.Low,
                                        Close = h.Close,
                                        Volume = h.Volume,
                                        LastUpdated = DateTimeUtils.GetEasternTime(),
                                    })
                                    .ToList()
                                : new List<MarketHistory>();

                        _db.MarketHistories.RemoveRange(
                            _db.MarketHistories.Where(m => m.Symbol == symbol)
                        );
                        _db.MarketHistories.AddRange(history);
                        await _db.SaveChangesAsync();
                    }
                    finally
                    {
                        _semaphore.Release();
                    }
                })
            );
        }
        await Task.WhenAll(tasks);
    }

    public async Task<List<StockScreenerResultDTO>> GetFilteredOptions(
        double minRor = 1.0,
        int rsiThreshold = 50,
        double bbLowerPercent = 33.0
    )
    {
        var chains = await _db.OptionChains.ToListAsync();
        var historyGroups = await _db
            .MarketHistories.GroupBy(m => m.Symbol)
            .ToDictionaryAsync(g => g.Key, g => g.ToList());

        var results = new List<StockScreenerResultDTO>();
        foreach (var kvp in historyGroups)
        {
            var symbol = kvp.Key;
            var history = kvp.Value.OrderByDescending(h => h.Date).ToList();
            var closes = history.Select(h => h.Close).ToArray();
            if (closes.Length < 200)
                continue; // Ensure enough data for SMAs

            var price = await GetCurrentPriceAsync(symbol);
            var rsi = CalculateRsi(closes.TakeLast(14 + 1).ToArray());
            var (upper, lower) = CalculateBollinger(closes.TakeLast(20).ToArray());
            var bbPercent = (price - lower) / (upper - lower) * 100;

            if (rsi < rsiThreshold && bbPercent < bbLowerPercent)
            {
                var puts = chains
                    .Where(c => c.Symbol == symbol && c.OptionType == "put" && c.Strike < price)
                    .ToList();
                var filteredPuts = puts.Where(p =>
                        CalculateRor((p.Bid + p.Ask) / 2, p.Strike) >= minRor
                    )
                    .Select(p => new FilteredOptionDTO
                    {
                        Strike = p.Strike,
                        Bid = p.Bid,
                        Ask = p.Ask,
                        Volume = p.Volume,
                        OpenInterest = p.OpenInterest,
                        ImpliedVol = p.ImpliedVol,
                        Delta = p.Delta,
                        // Add others
                    })
                    .ToList();

                results.Add(
                    new StockScreenerResultDTO
                    {
                        Symbol = symbol,
                        CurrentPrice = price,
                        RSI = rsi,
                        BBPercent = bbPercent,
                        SMAs = CalculateSma(closes),
                        FilteredPuts = filteredPuts,
                    }
                );
            }
        }
        return results;
    }

    private async Task<double> GetCurrentPriceAsync(string symbol)
    {
        var response = await _httpClient.GetAsync($"markets/quotes?symbols={symbol}");
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync();
        var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);
        if (
            dict == null
            || !dict.ContainsKey("quotes")
            || dict["quotes"].ValueKind != JsonValueKind.Array
            || dict["quotes"].GetArrayLength() == 0
        )
            throw new Exception("Invalid or missing quotes data from Tradier API.");
        var quoteData = dict["quotes"][0];
        return quoteData.GetProperty("last").GetDouble();
    }

    private double CalculateRor(double premium, double strike) => (premium / strike) * 100;

    private double CalculateRsi(double[] closes, int period = 14)
    {
        if (closes.Length < period + 1)
            return 0;
        var gains = new List<double>();
        var losses = new List<double>();
        for (int i = 1; i < closes.Length; i++)
        {
            var diff = closes[i] - closes[i - 1];
            if (diff > 0)
                gains.Add(diff);
            else
                losses.Add(Math.Abs(diff));
        }
        var avgGain = Statistics.Mean(gains.ToArray());
        var avgLoss = Statistics.Mean(losses.ToArray());
        if (avgLoss == 0)
            return 100;
        var rs = avgGain / avgLoss;
        return 100 - (100 / (1 + rs));
    }

    private (double Upper, double Lower) CalculateBollinger(double[] closes, int period = 20)
    {
        if (closes.Length < period)
            return (0, 0);
        var sma = Statistics.Mean(closes.TakeLast(period).ToArray());
        var stdDev = Statistics.StandardDeviation(closes.TakeLast(period).ToArray());
        return (sma + 2 * stdDev, sma - 2 * stdDev);
    }

    private (double S50, double S100, double S200) CalculateSma(double[] closes)
    {
        return (
            Statistics.Mean(closes.TakeLast(50).ToArray()),
            Statistics.Mean(closes.TakeLast(100).ToArray()),
            Statistics.Mean(closes.TakeLast(200).ToArray())
        );
    }
}
