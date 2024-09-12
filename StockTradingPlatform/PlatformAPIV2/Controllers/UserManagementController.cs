using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using UserPortfolioActor.Interfaces;
using UserProfile;

namespace PlatformAPIV2.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/users")]
    public class UserManagementController : ControllerBase
    {
        private IUserProfile GetUserProfileProxy()
        {
            return ServiceProxy.Create<IUserProfile>(
                new Uri("fabric:/StockTradingPlatform/UserProfile"), new ServicePartitionKey(0));
        }

        private IUserPortfolioActor GetUserPortfolioActorProxy(string username)
        {
            return ActorProxy.Create<IUserPortfolioActor>(
                new ActorId(username), new Uri("fabric:/StockTradingPlatform/UserPortfolioActorService"));
        }

        [HttpGet("{username}")]
        public async Task<IActionResult> GetUserProfile(string username)
        {
            var profile = await GetUserProfileProxy().GetUserProfileAsync(username);
            if (profile == null)
                return NotFound("User not found.");
            return Ok(profile);
        }

        [HttpGet("{username}/portfolio")]
        public async Task<IActionResult> GetUserPortfolio(string username)
        {
            var portfolioActor = GetUserPortfolioActorProxy(username);
            var portfolio = await portfolioActor.GetPortfolioAsync();
            return Ok(portfolio);
        }

        [HttpGet("{username}/portfolio/value")]
        public async Task<IActionResult> GetUserPortfolioValue(string username)
        {
            var portfolioActor = GetUserPortfolioActorProxy(username);
            var portfolioValue = await portfolioActor.GetPortfolioValueAsync();
            return Ok(portfolioValue);
        }
    }
}
