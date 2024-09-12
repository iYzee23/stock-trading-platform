using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using System.Security.Claims;
using TradingStateful;

namespace PlatformAPIV2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/trade")]
    public class TradingController : ControllerBase
    {
        private ITrading GetTradingProxy(string country)
        {
            return ServiceProxy.Create<ITrading>(
                new Uri("fabric:/StockTradingPlatform/TradingStateful"), new ServicePartitionKey(country));
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyStock([FromBody] TradeRequestDto request)
        {
            var country = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Country)?.Value;

            if (string.IsNullOrEmpty(country))
            {
                return Unauthorized("Invalid token.");
            }

            var order = new StockOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                Username = request.Username,
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                OrderType = "Buy"
            };

            await GetTradingProxy(country).EnqueueOrderAsync(order);
            return Ok($"Order to buy {request.Quantity} shares of {request.StockSymbol} has been placed successfully.");
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellStock([FromBody] TradeRequestDto request)
        {
            var country = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Country)?.Value;

            if (string.IsNullOrEmpty(country))
            {
                return Unauthorized("Invalid token.");
            }

            var order = new StockOrder
            {
                OrderId = Guid.NewGuid().ToString(),
                Username = request.Username,
                StockSymbol = request.StockSymbol,
                Quantity = request.Quantity,
                OrderType = "Sell"
            };

            await GetTradingProxy(country).EnqueueOrderAsync(order);
            return Ok($"Order to sell {request.Quantity} shares of {request.StockSymbol} has been placed successfully.");
        }

        [HttpGet("history/{username}")]
        public async Task<IActionResult> GetTransactionHistory(string username)
        {
            var country = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Country)?.Value;

            if (string.IsNullOrEmpty(country))
            {
                return Unauthorized("Invalid token.");
            }

            var history = await GetTradingProxy(country).GetTransactionHistoryAsync(username);
            return Ok(history);
        }
    }
}
