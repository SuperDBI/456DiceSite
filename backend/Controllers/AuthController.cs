using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            // TODO: Implement login logic (e.g., validate user credentials, generate JWT)
            return Ok(new { message = "Login successful" });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            // TODO: Implement sign-up logic (e.g., save user to database)
            return Ok(new { message = "Sign-up successful" });
        }

        [HttpPost("oauth-login")]
        public async Task<IActionResult> OAuthLogin([FromBody] OAuthLoginRequest request)
        {
            try
            {
                // Validate OAuth token and fetch user info
                var userInfo = await ValidateOAuthToken(request.Provider, request.Token);

                if (userInfo == null)
                {
                    return Unauthorized(new { message = "Invalid OAuth token" });
                }

                // Check if user exists in the database
                var user = _dbContext.Users.FirstOrDefault(u => u.Email == userInfo.Email);

                if (user == null)
                {
                    // Create a new user
                    user = new User
                    {
                        FullName = userInfo.FullName,
                        Email = userInfo.Email,
                        PasswordHash = null // No password for OAuth users
                    };

                    _dbContext.Users.Add(user);
                    await _dbContext.SaveChangesAsync();
                }

                // Generate JWT token
                var token = GenerateJwtToken(user);

                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred", error = ex.Message });
            }
        }

        private async Task<UserInfo> ValidateOAuthToken(string provider, string token)
        {
            // Example: Validate token with Google
            if (provider == "Google")
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(token);
                return new UserInfo
                {
                    FullName = payload.Name,
                    Email = payload.Email
                };
            }

            // Example: Validate token with Facebook
            if (provider == "Facebook")
            {
                // Call Facebook API to validate token and fetch user info
                // TODO: Implement Facebook token validation
            }

            return null;
        }

        private string GenerateJwtToken(User user)
        {
            // TODO: Implement JWT token generation
            return "sample-jwt-token";
        }

        public class UserInfo
        {
            public string FullName { get; set; }
            public string Email { get; set; }
        }

        public class LoginRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class SignUpRequest
        {
            public string Email { get; set; }
            public string Password { get; set; }
            public string FullName { get; set; }
        }
    }
}