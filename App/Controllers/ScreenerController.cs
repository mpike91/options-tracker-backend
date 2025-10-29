using Microsoft.AspNetCore.Mvc;
using ProverbsTrading.Models.DTOs;

[ApiController]
[Route("api/[controller]")]
public class ScreenerController : ControllerBase
{
    private readonly TradierService _tradierService;

    public ScreenerController(TradierService tradierService)
    {
        _tradierService = tradierService;
    }

    [HttpGet]
    public async Task<ActionResult<List<StockScreenerResultDTO>>> GetFilteredOptions(
        [FromQuery] double minRor = 1.0,
        [FromQuery] int rsiThreshold = 50,
        [FromQuery] double bbLowerPercent = 33.0)
    {
        try
        {
            if (minRor <= 0 || rsiThreshold <= 0 || bbLowerPercent < 0 || bbLowerPercent > 100)
            {
                return BadRequest("Invalid parameter values.");
            }

            var results = await _tradierService.GetFilteredOptions(minRor, rsiThreshold, bbLowerPercent);
            if (!results.Any())
            {
                return NoContent();  // Or Ok with empty list, depending on preference
            }
            return Ok(results);
        }
        catch (Exception ex)
        {
            // Log ex (e.g., via ILogger in production)
            return StatusCode(500, $"An error occurred while fetching screener data. Error: {ex.Message}");
        }
    }
}