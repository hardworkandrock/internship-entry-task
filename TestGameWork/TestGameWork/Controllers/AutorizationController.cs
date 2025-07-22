using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TestGameWork.DTOs;
using TestGameWork.Services;

namespace TestGameWork.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutorizationController : ControllerBase
    {
        private readonly IAutorizationService _authService;

        public AutorizationController(IAutorizationService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var response = await _authService.RegisterAsync(model);
            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var response = await _authService.LoginAsync(model);
            return Ok(response);
        }

        [HttpPost("test")]

        public async Task<IActionResult> Test()
        {
            var userId = User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier);
            return Ok();
        }
    }
}
