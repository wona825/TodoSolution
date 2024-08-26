using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Entites;
using Infrasfructure.Data;
using Infrasfructure.Error;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Infrasfructure.Repo
{
    internal class AuthRepo : IAuth
    {
        private readonly AppDbContext _appDbContext;
        private readonly IConfiguration _configuration;

        public AuthRepo(AppDbContext appDbContext, IConfiguration configuration)
        {
            this._appDbContext = appDbContext;
            this._configuration = configuration;
        }

        public async Task<LoginResponse> LoginUserAsync(LoginRequest loginRequest)
        {

            var getUser = await FindUserByUserName(loginRequest.UserName) ?? throw new CustomException(HttpStatusCode.Unauthorized, "Authenticate Fail");

            if (!BCrypt.Net.BCrypt.Verify(loginRequest.Password, getUser.HashPassword))
                throw new CustomException(HttpStatusCode.Unauthorized, "Wrong password.");

            string accessToken = GenerateAccessToken(getUser);

            Token? token = await _appDbContext.Tokens.FirstOrDefaultAsync(t => t.UserId == getUser.Id);

            string refreshToken = GenerateRefreshToken();
            if (token == null)
            {
                token = new Token()
                {
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.Now.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!)),
                    CreatedAt = DateTime.Now,
                    UserId = getUser.Id,
                    ApplicationUser = getUser
                };

                _appDbContext.Tokens.Add(token);
            }
            else
            {
                token.RefreshToken = refreshToken;
                token.CreatedAt = DateTime.Now;
                token.ExpiresAt = token.CreatedAt.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!));

                _appDbContext.Tokens.Update(token);
            }
            await _appDbContext.SaveChangesAsync();

            return new LoginResponse(getUser.Id, accessToken, token.RefreshToken);
        }

        private string GenerateAccessToken(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
            var userClaims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) };
            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: userClaims,
                expires: DateTime.Now.AddMinutes(int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"]!)),
                signingCredentials: credentials
               );
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];

            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);

                return Convert.ToBase64String(randomNumber);
            }
        }

        private async Task<ApplicationUser?> FindUserByUserName(string UserName) =>
            await _appDbContext.Users.FirstOrDefaultAsync(u => u.UserName == UserName);

        public async Task<RegisterUserResponse> RegisterUserAsync(RegisterUserRequest registerUserRequest)
        {
            var getUser = await FindUserByUserName(registerUserRequest.UserName);

            if (getUser != null)
                throw new CustomException(HttpStatusCode.Conflict, "User already exist");

            ApplicationUser user = new()                                                              
            {
                UserName = registerUserRequest.UserName,
                HashPassword = BCrypt.Net.BCrypt.HashPassword(registerUserRequest.Password),
                CreatedAt = DateTime.Now
            };
            await _appDbContext.SaveChangesAsync();

            string refreshToken = GenerateRefreshToken();

            _appDbContext.Tokens.Add(new Token()
            {
                RefreshToken = refreshToken,
                ExpiresAt = DateTime.Now.AddDays(int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"]!)),
                CreatedAt = DateTime.Now,
                UserId = user.Id,
                ApplicationUser = user
            });

            await _appDbContext.SaveChangesAsync();
            return new RegisterUserResponse(user.Id, GenerateAccessToken(user), refreshToken);
        }

        public async Task<RefreshTokenResponse> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var storedToken = await _appDbContext.Tokens
                .Include(t => t.ApplicationUser)
                .SingleOrDefaultAsync(t => t.RefreshToken == refreshTokenRequest.RefreshToken);

            if (storedToken == null || storedToken.ExpiresAt <= DateTime.Now || storedToken.RevokedAt != null)
            {
                throw new CustomException(HttpStatusCode.Unauthorized ,"Invalid refresh token");
            }

            var principal = GetPrincipalFromExpiredToken(refreshTokenRequest.AccessToken);
            var userId = int.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)!.Value);
            if (storedToken.UserId != userId)
                throw new CustomException(HttpStatusCode.Unauthorized, "Invalid access token");

            var newAccessToken = GenerateAccessToken(storedToken.ApplicationUser);

            await _appDbContext.SaveChangesAsync();

            return new RefreshTokenResponse(newAccessToken);
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;

            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new CustomException(HttpStatusCode.Unauthorized, "Invalid access token");

            return principal;
        }
    }
}
