using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[ApiController]
[Route("api/finance")]
public class FinanceController : ControllerBase {

    // [HttpGet("stock/{symbol}")]
    // public async Task<IActionResult> GetStockData(string symbol) {
    //     try {
    //         using (var client = new AlphaVantageClient(ApiKey)) {
    //             using (var stocksClient = client.Stocks()) {
    //                 // Quote (prices)
    //                 var quote = await stocksClient.GetGlobalQuoteAsync(symbol);

    //                 // Historical (last month, daily)
    //                 var historical = await stocksClient.GetTimeSeriesAsync(symbol, TimeSeriesInterval.Daily, TimeSeriesSize.Compact);  // Compact for last 100 days

    //                 // Options chain: Limited in free tier; use quote for basic, or premium for full
    //                 // For now, placeholder (upgrade for ALPHA_VANTAGE_OPTION_CHAIN if needed)
    //                 var options = "Free tier limited; use premium for full chains";

    //                 // Fundamentals (overview, income, balance, cash flow for Stock Score)
    //                 var companyOverview = await stocksClient.GetCompanyOverviewAsync(symbol);
    //                 var incomeStatement = await stocksClient.GetIncomeStatementAsync(symbol);
    //                 var balanceSheet = await stocksClient.GetBalanceSheetAsync(symbol);
    //                 var cashFlow = await stocksClient.GetCashFlowAsync(symbol);

    //                 // Technicals (RSI, Bollinger for scanner)
    //                 var rsi = await stocksClient.GetTechnicalIndicatorAsync(symbol, IndicatorType.RSI, TimeSeriesInterval.Daily, 14);  // RSI(14)
    //                 var bollinger = await stocksClient.GetTechnicalIndicatorAsync(symbol, IndicatorType.BBANDS, TimeSeriesInterval.Daily, 20);  // BBANDS(20)

    //                 return Ok(new { Quote = quote, Historical = historical, Options = options, Fundamentals = new { CompanyOverview = companyOverview, Income = incomeStatement, Balance = balanceSheet, CashFlow = cashFlow }, Technicals = new { RSI = rsi, Bollinger = bollinger } });
    //             }
    //         }
    //     } catch (Exception ex) {
    //         return BadRequest(ex.Message);
    //     }
    // }

    // [HttpPost("stocks")]
    // public async Task<IActionResult> GetMultipleStocks([FromBody] List<string> symbols) {
    //     var results = new Dictionary<string, object>();
    //     foreach (var symbol in symbols) {
    //         var response = await GetStockData(symbol);
    //         if (response is OkObjectResult okResult) {
    //             results[symbol] = okResult.Value;
    //         } else {
    //             results[symbol] = "Error fetching data";
    //         }
    //     }
    //     return Ok(results);
    // }
}