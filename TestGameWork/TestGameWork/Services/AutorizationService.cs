using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.EntityFrameworkCore;
using TestGameWork.Data;
using TestGameWork.DTOs; 
using TestGameWork.Models;

namespace TestGameWork.Services
{
    public class AutorizationService : IAutorizationService
    {
        private readonly AppDbContext _db;
        private readonly IConfiguration _config;

        public AutorizationService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        public async Task<AutorizationResponse> LoginAsync(LoginModel model)
        {
            // Поиск игрока
            var player = await _db.Players
                .FirstOrDefaultAsync(p => p.Name == model.Name);
             
            if (player == null)
                throw new UnauthorizedAccessException("Пользователь не найден");
             
            if (!BCrypt.Net.BCrypt.Verify(model.Password, player.HashPassword))
                throw new UnauthorizedAccessException("Неверный пароль");

            // JWT
            var token = GenerateJwtToken(player);

            // Ok
            return new AutorizationResponse
            {
                Token = token,
                Id = player.Id,
                Name = player.Name
            };
        }

        public async Task<AutorizationResponse> RegisterAsync(RegisterModel model)
        {
            if (await _db.Players.AnyAsync(u => u.Name == model.Name))
                throw new InvalidOperationException("Пользователь уже существует");

            var player = new Player
            {
                Id = Guid.NewGuid(),
                Name = model.Name,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(model.Password)
            };

            await _db.Players.AddAsync(player);
            await _db.SaveChangesAsync();

            var token = GenerateJwtToken(player);
            return new AutorizationResponse
            {
                Token = token,
                Id = player.Id,
                Name = player.Name
            };
        }

        private string GenerateJwtToken(Player player)
        {
            var jwtSettings = _config.GetSection("JwtSettings");
            var secret = jwtSettings.GetValue<string>("Secret");
            var issuer = jwtSettings.GetValue<string>("Issuer");
            var audience = jwtSettings.GetValue<string>("Audience");
            var lifetime = jwtSettings.GetValue<int>("TokenLifetimeMinutes");

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
            new Claim(ClaimTypes.Name, player.Name),
            new Claim(ClaimTypes.NameIdentifier, player.Id.ToString())
        };

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.Now.AddMinutes(lifetime),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

    }
}
