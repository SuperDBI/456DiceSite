using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Text;

namespace Backend.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly string GoogleClientId = "1076399293244-t9eiketf9706egvskuh6vfgvranttmrk.apps.googleusercontent.com";
        private readonly string FacebookAppId = "4380271422242547"; // Replace with actual Facebook App ID
        private readonly string FacebookAppSecret = "e9bcff561a29396b4858e3f9460963cf"; // Replace with actual Facebook App Secret
        private readonly string JwtSecret = "Y0z3EU+9HIsZdRJKlvjRJlqZAhs6iuV09IXkrVj0U4w="; // Secure JWT secret

        [HttpPost("validate-google-token")]
        public async Task<IActionResult> ValidateGoogleToken([FromBody] string idToken)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(idToken);

            if (jwtToken == null || jwtToken.Payload["aud"].ToString() != GoogleClientId)
            {
                return Unauthorized("Invalid Google token.");
            }

            return Ok(new { Message = "Google token is valid." });
        }

        [HttpPost("validate-facebook-token")]
        public async Task<IActionResult> ValidateFacebookToken([FromBody] string accessToken)
        {
            using (var httpClient = new HttpClient())
            {
                var appAccessTokenResponse = await httpClient.GetStringAsync($"https://graph.facebook.com/oauth/access_token?client_id={FacebookAppId}&client_secret={FacebookAppSecret}&grant_type=client_credentials");
                var appAccessToken = JObject.Parse(appAccessTokenResponse)["access_token"].ToString();

                var debugTokenResponse = await httpClient.GetStringAsync($"https://graph.facebook.com/debug_token?input_token={accessToken}&access_token={appAccessToken}");
                var debugToken = JObject.Parse(debugTokenResponse);

                if (debugToken["data"]?["is_valid"]?.ToObject<bool>() != true)
                {
                    return Unauthorized("Invalid Facebook token.");
                }

                return Ok(new { Message = "Facebook token is valid." });
            }
        }

        [HttpPost("generate-jwt")]
        public IActionResult GenerateJwt([FromBody] string userId)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(JwtSecret);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return Ok(new { Token = tokenHandler.WriteToken(token) });
        }
    }
}