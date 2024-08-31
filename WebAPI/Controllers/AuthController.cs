using System.Net;
using System.Security.Claims;
using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Domain.Entites;
using Infrasfructure.Error;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuth auth) : ControllerBase
    {
        private readonly IAuth _auth = auth;

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> LoginAsync(LoginRequest loginRequest)
        {
            var result = await _auth.LoginAsync(loginRequest);
            return Ok(result);
        }

        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> RegisterUserAsync(RegisterUserRequest registerUserRequest)
        {
            var result = await _auth.RegisterUserAsync(registerUserRequest);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest refreshTokenRequest)
        {
            var result = await _auth.RefreshTokenAsync(refreshTokenRequest);
            return Ok(result);
        }

        [HttpPost("logout"), Authorize]
        public async Task<ActionResult> LogoutAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? throw new CustomException(HttpStatusCode.Unauthorized, "User ID not found in token."));

            await _auth.LogoutAsync(userId);
            return Ok();
        }
    }
}
