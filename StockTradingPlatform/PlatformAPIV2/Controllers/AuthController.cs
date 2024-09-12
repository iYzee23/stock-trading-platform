using Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;

namespace PlatformAPIV2.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private IAuthentication GetAuthProxy()
        {
            return ServiceProxy.Create<IAuthentication>(
                new Uri("fabric:/StockTradingPlatform/Authentication"));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var result = await GetAuthProxy().RegisterUserAsync(registerDto.Username, registerDto.Name, registerDto.Password, registerDto.Email, registerDto.Country);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var token = await GetAuthProxy().LoginUserAsync(loginDto.Username, loginDto.Password);
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized("Invalid credentials");
            }
            return Ok(new { Token = token });
        }

        [HttpGet("validate-token")]
        public async Task<IActionResult> ValidateToken([FromHeader] string token)
        {
            var isValid = await GetAuthProxy().ValidateTokenAsync(token);
            if (!isValid)
            {
                return Unauthorized("Invalid token");
            }
            return Ok("Token is valid");
        }
    }
}
