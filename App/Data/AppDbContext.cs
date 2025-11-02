using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProverbsTrading.Models;

public class AppDbContext : IdentityDbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options) { }

    public DbSet<WeeklyStock> WeeklyStocks { get; set; }
    public DbSet<OptionExpiration> OptionExpirations { get; set; }
    public DbSet<OptionChain> OptionChains { get; set; }
    public DbSet<MarketHistory> MarketHistories { get; set; }
}
