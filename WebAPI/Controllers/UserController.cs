using System.Net;
using System.Security.Claims;
using Application.Contracts;
using Infrasfructure.Error;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController(IUser user) : ControllerBase
	{
        private readonly IUser _user = user;

        [HttpGet("nickname/is-exist")]
        public async Task<ActionResult> CheckUserNameDuplicationAsync([FromQuery(Name = "username")] string? userName)
        {
            bool result = await _user.CheckUserNameDuplicationAsync(userName);
            return Ok(new { exist = result });
        }

        [HttpDelete(), Authorize]
        public async Task<ActionResult> SoftDeleteUserAsync()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value
            ?? throw new CustomException(HttpStatusCode.Unauthorized, "User ID not found in token."));

            await _user.SoftDeleteUserAsync(userId);
            return Ok();
        }
    }
}