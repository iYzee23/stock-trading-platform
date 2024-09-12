using LeaderboardStateless;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace PlatformAPIV2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/leaderboard")]
    public class LeaderboardController : ControllerBase
    {
        private ILeaderboard GetLeaderboardProxy()
        {
            return ServiceProxy.Create<ILeaderboard>(
                new Uri("fabric:/StockTradingPlatform/LeaderboardStateless"));
        }

        [HttpGet]
        public async Task<IActionResult> GetLeaderboard()
        {
            var leaderboard = await GetLeaderboardProxy().GetLeaderboardAsync();
            return Ok(leaderboard);
        }
    }
}
