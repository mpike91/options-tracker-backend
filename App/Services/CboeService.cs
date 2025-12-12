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

    public async Task<List<WeeklyOptionableStockDTO>> GetWeeklyOptionableStocksAsync()
    {
        var stocks = await _db
            .WeeklyOptionableStocks.Select(s => new WeeklyOptionableStockDTO
            {
                Symbol = s.Symbol,
                Name = s.Name,
                LastUpdated = DateTimeUtils.ToEasternTime(s.LastUpdated),
            })
            .ToListAsync();

        return stocks;
    }

    public async Task<WeeklyOptionableStocksCommaSeparatedDTO> GetWeeklyOptionableStocksCommaSeparatedListAsync()
    {
        var stocks = await GetWeeklyOptionableStocksAsync();
        var commaSeparatedSymbols = string.Join(",", stocks.Select(stock => stock.Symbol));

        return new WeeklyOptionableStocksCommaSeparatedDTO
        {
            Count = stocks.Count,
            CommaSeparatedList = commaSeparatedSymbols,
        };
    }

    public async Task UpdateWeeklyOptionableStocksAsync()
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
                    var newStocks = new List<WeeklyOptionableStock>();
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
                                    new WeeklyOptionableStock
                                    {
                                        Symbol = symbol,
                                        Name = name,
                                        LastUpdated = DateTime.UtcNow, // Store in UTC; convert on retrieval if needed
                                    }
                                );
                            }
                        }
                    }

                    var existing = await _db.WeeklyOptionableStocks.ToListAsync();
                    _db.WeeklyOptionableStocks.RemoveRange(
                        existing.Where(e => !newStocks.Any(n => n.Symbol == e.Symbol))
                    );
                    _db.WeeklyOptionableStocks.AddRange(
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
