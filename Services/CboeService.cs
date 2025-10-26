using CsvHelper;
using ProverbsTrading.Models;
using System.Globalization;
using System.IO;
using System.Net.Http;
using Microsoft.EntityFrameworkCore;

public class CboeService {
    private readonly AppDbContext _db;
    private readonly HttpClient _httpClient = new HttpClient();

    public CboeService(AppDbContext db) {
        _db = db;
    }

    public async Task UpdateWeeklyStocksAsync() {
        var csvUrl = "https://www.cboe.com/available_weeklys/get_csv_download/";
        var response = await _httpClient.GetAsync(csvUrl);
        response.EnsureSuccessStatusCode();
        var csvStream = await response.Content.ReadAsStreamAsync();

        using (var reader = new StreamReader(csvStream)) {
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture)) {
                csv.Read(); csv.ReadHeader();  // Skip header
                var newStocks = new List<WeeklyStock>();
                while (csv.Read()) {
                    newStocks.Add(new WeeklyStock {
                        Symbol = csv.GetField("Symbol") ?? string.Empty,
                        Name = csv.GetField("Name") ?? string.Empty,
                        LastUpdated = DateTime.UtcNow
                    });
                }

                var existing = await _db.WeeklyStocks.ToListAsync();
                _db.WeeklyStocks.RemoveRange(existing.Where(e => !newStocks.Any(n => n.Symbol == e.Symbol)));
                _db.WeeklyStocks.AddRange(newStocks.Where(n => !existing.Any(e => e.Symbol == n.Symbol)));
                await _db.SaveChangesAsync();
            }
        }
    }
}