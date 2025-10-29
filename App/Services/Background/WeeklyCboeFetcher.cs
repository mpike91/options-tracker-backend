using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ProverbsTrading.Services.Background;

public class WeeklyCboeFetcher : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;  // To create scopes
    private readonly ILogger<WeeklyCboeFetcher> _logger;

    public WeeklyCboeFetcher(IServiceProvider serviceProvider, ILogger<WeeklyCboeFetcher> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var now = DateTimeUtils.GetEasternTime();  // Use utils
            if (now.DayOfWeek == DayOfWeek.Saturday && now.Hour == 0 && now.Minute == 0)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var cboeService = scope.ServiceProvider.GetRequiredService<CboeService>();
                    await cboeService.UpdateWeeklyStocksAsync();
                    _logger.LogInformation("Weekly CBOE stocks updated at {Time}", now);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error during weekly CBOE update.");
                }
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);  // Check every minute
        }
    }
}