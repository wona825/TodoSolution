using Application.Contracts;
using Application.DTOs.Request;
using Application.DTOs.Response;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuth auth) : ControllerBase
    {
        private readonly IAuth _auth = auth;

        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> LogUserIn(LoginRequest loginRequest)
        {
            var result = await _auth.LoginUserAsync(loginRequest);
            return Ok(result);
        }


        [HttpPost("register")]
        public async Task<ActionResult<LoginResponse>> RegisterUser(RegisterUserRequest registerUserRequest)
        {
            var result = await _auth.RegisterUserAsync(registerUserRequest);
            return Ok(result);
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(RefreshTokenRequest refreshTokenRequest)
        {
            var result = await _auth.RefreshTokenAsync(refreshTokenRequest);
            return Ok(result);
        }
    }
}
