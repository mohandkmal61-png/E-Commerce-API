using ECommerce.Domin.Entitie.Identitymodule;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace ECommerce.Services.TokenService
{
    public class TokenService
    {
        private readonly IConfiguration _configuration;

        public TokenService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateToken(ApplicationUser user, IList<string> roles)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenSecret = _configuration["JwtSettings:Secret"]
                ?? throw new InvalidOperationException("JWT secret is not configured.");
            var key = Encoding.ASCII.GetBytes(tokenSecret);
            var expirationHours = _configuration.GetValue<double?>("JwtSettings:ExpirationHours") ?? 24;
            var issuer = _configuration["JwtSettings:Issuer"]
                ?? throw new InvalidOperationException("JWT issuer is not configured.");
            var audience = _configuration["JwtSettings:Audience"]
                ?? throw new InvalidOperationException("JWT audience is not configured.");

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, user.Id ?? string.Empty),
                new(ClaimTypes.Email, user.Email ?? string.Empty),
                new(ClaimTypes.Name, user.UserName ?? user.Email ?? string.Empty)
            };

            foreach (var role in roles.Where(role => !string.IsNullOrWhiteSpace(role)))
            {
                claims.Add(new(ClaimTypes.Role, role));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(expirationHours),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
