using System;
using System.Linq;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public AuthController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!IsValidEmail(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Email and password are required." });
            }

            var email = request.Email.Trim().ToLowerInvariant();
            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null || !VerifyPasswordHash(request.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid email or password." });
            }

            return Ok(new
            {
                message = "Login successful",
                token = CreateToken(user.Email),
                user = new { user.FullName, user.Email }
            });
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpRequest request)
        {
            if (!IsValidEmail(request.Email) || string.IsNullOrWhiteSpace(request.Password) || string.IsNullOrWhiteSpace(request.FullName))
            {
                return BadRequest(new { message = "Full name, email, and password are required." });
            }

            var email = request.Email.Trim().ToLowerInvariant();
            if (await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == email))
            {
                return Conflict(new { message = "An account with that email already exists." });
            }

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = email,
                PasswordHash = CreatePasswordHash(request.Password)
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                message = "Sign-up successful",
                token = CreateToken(user.Email),
                user = new { user.FullName, user.Email }
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) && string.IsNullOrWhiteSpace(request.PlayerId))
            {
                return BadRequest(new { message = "Email or playerId is required to reset password." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword))
            {
                return BadRequest(new { message = "A new password is required." });
            }

            if (string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(request.PlayerId))
            {
                email = $"{request.PlayerId.Trim().ToLowerInvariant()}@456dice.com";
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
            {
                return NotFound(new { success = false, message = "No account found for that email." });
            }

            user.PasswordHash = CreatePasswordHash(request.NewPassword);
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();

            return Ok(new { success = true, message = "Password reset successful." });
        }

        [HttpPost("portal-link")]
        public async Task<IActionResult> PortalLink([FromBody] PortalLinkRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email) && !string.IsNullOrWhiteSpace(request.Username))
            {
                email = $"{request.Username.Trim().ToLowerInvariant()}@456dice.com";
            }

            if (!IsValidEmail(email) || string.IsNullOrWhiteSpace(request.Password))
            {
                return BadRequest(new { message = "Username/email and password are required." });
            }

            var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email.ToLower() == email);
            if (user == null)
            {
                user = new User
                {
                    FullName = string.IsNullOrWhiteSpace(request.DisplayName)
                        ? request.Username ?? request.Email
                        : request.DisplayName.Trim(),
                    Email = email,
                    PasswordHash = CreatePasswordHash(request.Password)
                };
                _dbContext.Users.Add(user);
            }
            else
            {
                user.PasswordHash = CreatePasswordHash(request.Password);
                _dbContext.Users.Update(user);
            }

            await _dbContext.SaveChangesAsync();

            var token = CreateToken(user.Email);
            var portalUrl = $"https://456dice.com/portal?token={Uri.EscapeDataString(token)}";

            return Ok(new
            {
                message = "Portal account linked successfully.",
                success = true,
                token,
                portalUrl
            });
        }

        private static bool IsValidEmail(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return false;
            }

            try
            {
                var address = new MailAddress(email.Trim());
                return address.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        private static string CreatePasswordHash(string password)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[16];
            rng.GetBytes(salt);

            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var hash = deriveBytes.GetBytes(32);
            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(hash)}";
        }

        private static bool VerifyPasswordHash(string password, string storedHash)
        {
            if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
            {
                return false;
            }

            var parts = storedHash.Split(':');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var hash = Convert.FromBase64String(parts[1]);

            using var deriveBytes = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256);
            var computedHash = deriveBytes.GetBytes(hash.Length);
            return computedHash.SequenceEqual(hash);
        }

        private static string CreateToken(string email)
        {
            var token = Guid.NewGuid().ToString("N");
            return Convert.ToBase64String(Encoding.UTF8.GetBytes($"{token}:{email}:{DateTime.UtcNow:o}"));
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

        public class ResetPasswordRequest
        {
            public string? Email { get; set; }
            public string? PlayerId { get; set; }
            public string? NewPassword { get; set; }
        }

        public class PortalLinkRequest
        {
            public string? Username { get; set; }
            public string? Email { get; set; }
            public string Password { get; set; } = string.Empty;
            public string? PlayerId { get; set; }
            public string? DeviceId { get; set; }
            public string? DisplayName { get; set; }
        }
    }
}
