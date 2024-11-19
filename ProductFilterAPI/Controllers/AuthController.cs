using Microsoft.AspNetCore.Mvc;
using ProductFilterAPI.Models;
using ProductFilterAPI.Services;

namespace ProductFilterAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthenticationService _authService;

        public AuthController(IAuthenticationService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginModel model)
        {
            if (IsValidUser(model.Username, model.Password))
            {
                var token = _authService.GenerateToken(model.Username);
                return Ok(new { Token = token });
            }

            return Unauthorized();
        }

        private bool IsValidUser(string username, string password)
        {
            return username == "admin" && password == "password123";
        }
    }
} 