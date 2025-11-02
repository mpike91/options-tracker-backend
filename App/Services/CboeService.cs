using System.Globalization;
using CsvHelper;
using Microsoft.EntityFrameworkCore;
using ProverbsTrading.Models;
using ProverbsTrading.Models.DTOs;

public class CboeService
{
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient;

    public CboeService(AppDbContext db, HttpClient httpClient)
    {
        _db = db;
        _httpClient = httpClient;
    }

    public async Task<List<WeeklyStockDTO>> GetWeeklyStocksAsync()
    {
        var stocks = await _db
            .WeeklyStocks.Select(s => new WeeklyStockDTO
            {
                Symbol = s.Symbol,
                Name = s.Name,
                LastUpdated = DateTimeUtils.ToEasternTime(s.LastUpdated),
            })
            .ToListAsync();

        return stocks;
    }

    public async Task<WeeklyStockCommaSeparatedListDTO> GetWeeklyStocksCommaSeparatedListAsync()
    {
        var stocks = await GetWeeklyStocksAsync();
        var commaSeparatedSymbols = string.Join(",", stocks.Select(stock => stock.Symbol));

        return new WeeklyStockCommaSeparatedListDTO
        {
            SymbolCount = stocks.Count,
            CommaSeparatedSymbolList = commaSeparatedSymbols,
        };
    }

    public async Task UpdateWeeklyStocksAsync()
    {
        var csvUrl = "https://www.cboe.com/available_weeklys/get_csv_download/";
        try
        {
            var response = await _httpClient.GetAsync(csvUrl);
            response.EnsureSuccessStatusCode();
            var csvStream = await response.Content.ReadAsStreamAsync();

            using (var reader = new StreamReader(csvStream))
            {
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    // No ReadHeader() - parse index-based to handle irregular structure
                    var newStocks = new List<WeeklyStock>();
                    while (csv.Read())
                    {
                        string? symbol = null;
                        string? name = null;
                        if (
                            csv.TryGetField(0, out symbol)
                            && csv.TryGetField(1, out name)
                            && !string.IsNullOrWhiteSpace(symbol)
                            && !string.IsNullOrWhiteSpace(name)
                            && !csv.TryGetField<string?>(2, out _)
                        )
                        { // Ensure no third field (discard out)
                            // Validate symbol-like (uppercase alphanum, short length) and name has letters
                            if (
                                symbol != null
                                && symbol.Length <= 10
                                && symbol.All(c => char.IsUpper(c) || char.IsDigit(c))
                                && name != null
                                && name.Any(char.IsLetter)
                            )
                            {
                                newStocks.Add(
                                    new WeeklyStock
                                    {
                                        Symbol = symbol,
                                        Name = name,
                                        LastUpdated = DateTime.UtcNow, // Store in UTC; convert on retrieval if needed
                                    }
                                );
                            }
                        }
                    }

                    var existing = await _db.WeeklyStocks.ToListAsync();
                    _db.WeeklyStocks.RemoveRange(
                        existing.Where(e => !newStocks.Any(n => n.Symbol == e.Symbol))
                    );
                    _db.WeeklyStocks.AddRange(
                        newStocks.Where(n => !existing.Any(e => e.Symbol == n.Symbol))
                    );
                    await _db.SaveChangesAsync();
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception("Failed to update weekly stocks from CBOE.", ex);
        }
    }
}
