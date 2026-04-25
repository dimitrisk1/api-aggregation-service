using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly IIdentityService _identityService;

        public AuthController(IIdentityService identityService)
        {
            _identityService = identityService;
        }

        [HttpPost("token")]
        public IActionResult GetToken([FromBody] TokenRequest? request)
        {
            var username = string.IsNullOrWhiteSpace(request?.Username) ? "demo-user" : request.Username;
            var role = string.IsNullOrWhiteSpace(request?.Role) ? "User" : request.Role;
            var token = _identityService.GenerateToken(username, role);

            return Ok(new { Token = token });
        }

        public sealed record TokenRequest(string? Username, string? Role);
    }
}
