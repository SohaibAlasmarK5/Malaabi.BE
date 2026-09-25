using Mal3abi.Core.Interfaces.Services;
using Mal3abi.Core.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Mal3abi.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST /api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseResource>> Register([FromBody] RegisterRequestResource request)
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(result);
        }

        // POST /api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseResource>> Login([FromBody] LoginRequestResource request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }

        // GET /api/auth/me
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<UserResource>> Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (userId is null) return Unauthorized();

            var user = await _authService.GetByIdAsync(userId);
            return user is null ? NotFound() : Ok(user);
        }
    }
}