using Aggregator;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using UserProfile;

namespace PlatformAPIV2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/stocks")]
    public class StockPricesController : ControllerBase
    {
        private IAggregator GetAggregatorProxy()
        {
            return ServiceProxy.Create<IAggregator>(
                new Uri("fabric:/StockTradingPlatform/Aggregator"));
        }

        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestStockPrices()
        {
            var stockPrices = await GetAggregatorProxy().GetAllStockPricesAsync();
            if (stockPrices == null)
                return NotFound("Stock prices not found.");
            return Ok(stockPrices);
        }

        [HttpGet("symbols")]
        public async Task<IActionResult> GetSupportedSymbols()
        {
            var symbols = await GetAggregatorProxy().GetSupportedSymbols();
            if (symbols == null)
                return NotFound("No supported symbols found.");
            return Ok(symbols);
        }
    }
}
