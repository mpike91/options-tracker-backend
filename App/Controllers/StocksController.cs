using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProverbsTrading.Models.DTOs;

[ApiController]
[Route("api/[controller]")]
public class DataController : ControllerBase
{
    private readonly CboeService _cboeService;
    private readonly TradierService _tradierService;

    public DataController(AppDbContext db, CboeService cboeService, TradierService tradierService)
    {
        _cboeService = cboeService;
        _tradierService = tradierService;
    }

    [HttpGet("stocks")]
    public async Task<ActionResult<List<WeeklyOptionableStockDTO>>> GetWeeklyOptionableStocks()
    {
        try
        {
            var stocks = await _cboeService.GetWeeklyOptionableStocksAsync();
            return Ok(stocks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching weekly stocks: {ex.Message}");
        }
    }

    [HttpGet("stocks/comma-separated-list")]
    public async Task<
        ActionResult<WeeklyOptionableStocksCommaSeparatedDTO>
    > GetWeeklyOptionableStocksCommaSeparatedList()
    {
        try
        {
            var stocks = await _cboeService.GetWeeklyOptionableStocksCommaSeparatedListAsync();
            return Ok(stocks);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error fetching weekly stocks: {ex.Message}");
        }
    }

    [HttpPost("stocks/cboe")]
    public async Task<ActionResult> FetchCboeWeeklyOptionableStocks()
    {
        try
        {
            await _cboeService.UpdateWeeklyOptionableStocksAsync();
            return Ok("Weekly stocks updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating weekly stocks: {ex.Message}");
        }
    }

    [HttpPost("stocks/stock-type")]
    public async Task<ActionResult> FetchWeeklyOptionableStocksTypes()
    {
        try
        {
            await _tradierService.UpdateWeeklyOptionableStocksTypesAsync();
            return Ok("Weekly stocks types updated successfully.");
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Error updating weekly stocks types: {ex.Message}");
        }
    }

    // // Trigger Tradier expirations fetch
    // [HttpPost("fetch/expirations")]
    // public async Task<ActionResult> FetchExpirations()
    // {
    //     try
    //     {
    //         await _tradierService.FetchExpirationsForAllStocksAsync();
    //         return Ok("Option expirations fetched successfully.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Error fetching expirations: {ex.Message}");
    //     }
    // }

    // // Trigger Tradier chains fetch
    // [HttpPost("fetch/chains")]
    // public async Task<ActionResult> FetchChains()
    // {
    //     try
    //     {
    //         await _tradierService.FetchChainsForAllStocksAsync();
    //         return Ok("Option chains fetched successfully.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Error fetching chains: {ex.Message}");
    //     }
    // }

    // // Trigger Tradier history fetch
    // [HttpPost("fetch/history")]
    // public async Task<ActionResult> FetchHistory()
    // {
    //     try
    //     {
    //         await _tradierService.FetchHistoryForAllStocksAsync();
    //         return Ok("Market history fetched successfully.");
    //     }
    //     catch (Exception ex)
    //     {
    //         return StatusCode(500, $"Error fetching history: {ex.Message}");
    //     }
    // }
}
