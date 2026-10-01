using Application.DTOs;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Domain.Entities;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<UserEntity> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<UserEntity> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<TokenResponse> LoginAsync(LoginRequest request, CancellationToken ct = default)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, request.Password))
            {
                throw new UnauthorizedAccessException("Неверный email или пароль");
            }

            var (token, refreshToken) = await GenerateTokenPairAsync(user);
            await UpdateRefreshTokenAsync(user, refreshToken);

            var roles = await _userManager.GetRolesAsync(user);

            return new TokenResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            };
        }

        public async Task<TokenResponse> RegisterAsync(RegisterRequest request, CancellationToken ct = default)
        {
            var user = new UserEntity
            {
                UserName = request.UserName,
                Email = request.Email,
                NormalizedEmail = request.Email.ToUpper(),
                NormalizedUserName = request.UserName.ToUpper()
            };

            var result = await _userManager.CreateAsync(user, request.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Ошибка регистрации: {errors}");
            }

            await _userManager.AddToRoleAsync(user, "user");

            var (token, refreshToken) = await GenerateTokenPairAsync(user);
            await UpdateRefreshTokenAsync(user, refreshToken);

            var roles = await _userManager.GetRolesAsync(user);

            return new TokenResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserName = user.UserName!,
                Email = user.Email!,
                Roles = roles.ToList()
            };
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken ct = default)
        {
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
            var userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var user = await _userManager.FindByIdAsync(userId!);
            if (user == null || user.RefreshToken != request.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            {
                throw new UnauthorizedAccessException("Недействительный токен");
            }

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());

            await _userManager.UpdateAsync(user);

            var newToken = await GenerateJwtTokenAsync(user);

            return new RefreshTokenResponse
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            };
        }

        private async Task<(string token, string refreshToken)> GenerateTokenPairAsync(UserEntity user)
        {
            var token = await GenerateJwtTokenAsync(user);
            var refreshToken = GenerateRefreshToken();
            return (token, refreshToken);
        }

        private async Task UpdateRefreshTokenAsync(UserEntity user, string refreshToken)
        {
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(GetRefreshTokenExpiryDays());
            await _userManager.UpdateAsync(user);
        }

        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }

        private int GetRefreshTokenExpiryDays()
        {
            return int.Parse(_configuration["Jwt:RefreshTokenDays"] ?? "7");
        }

        private async Task<string> GenerateJwtTokenAsync(UserEntity user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email!),
                new Claim(ClaimTypes.Name, user.UserName!)
            };

            var roles = await _userManager.GetRolesAsync(user);
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = true,
                ValidateIssuer = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Неверный формат токена");
            }

            return principal;
        }
    }
}
