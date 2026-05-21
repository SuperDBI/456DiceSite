using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            return Ok(new { message = "Login successful", token = "sample-jwt-token" });
        }

        [HttpPost("signup")]
        public IActionResult SignUp([FromBody] SignUpRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required" });
            }

            return Ok(new { message = "Sign-up successful" });
        }

        [HttpPost("oauth-login")]
        public IActionResult OAuthLogin([FromBody] OAuthLoginRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Provider) || string.IsNullOrWhiteSpace(request.Token))
            {
                return BadRequest(new { message = "Provider and token are required" });
            }

            return Ok(new { message = "OAuth login successful", token = "sample-jwt-token" });
        }

        public class OAuthLoginRequest
        {
            public string Provider { get; set; } = string.Empty;
            public string Token { get; set; } = string.Empty;
        }

        public class LoginRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class SignUpRequest
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string FullName { get; set; } = string.Empty;
        }
    }
}