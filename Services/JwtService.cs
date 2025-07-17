using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using NGO_WebAPI_Backend.Models;

namespace NGO_WebAPI_Backend.Services
{
    public interface IJwtService
    {
        string GenerateToken(Worker worker);
        ClaimsPrincipal? ValidateToken(string token);
        int? GetWorkerIdFromToken(string token);
    }

    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<JwtService> _logger;

        public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public string GenerateToken(Worker worker)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetJwtKey()));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, worker.WorkerId.ToString()),
                new Claim(ClaimTypes.Name, worker.Name ?? ""),
                new Claim(ClaimTypes.Email, worker.Email ?? ""),
                new Claim(ClaimTypes.Role, worker.Role ?? "staff"),
                new Claim("WorkerId", worker.WorkerId.ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8), // 8小時有效期
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? ValidateToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.UTF8.GetBytes(GetJwtKey());

                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = _configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = _configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                return principal;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "JWT token 驗證失敗");
                return null;
            }
        }

        public int? GetWorkerIdFromToken(string token)
        {
            var principal = ValidateToken(token);
            if (principal == null)
                return null;

            var workerIdClaim = principal.FindFirst("WorkerId");
            if (workerIdClaim != null && int.TryParse(workerIdClaim.Value, out int workerId))
            {
                return workerId;
            }

            return null;
        }

        private string GetJwtKey()
        {
            return _configuration["Jwt:Key"] ?? "your-super-secret-key-that-should-be-at-least-32-characters-long";
        }
    }
}