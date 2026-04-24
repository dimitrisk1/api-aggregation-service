using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        public AuthController()
        {
        }

        [HttpPost("token")]
        public IActionResult GetToken()
        {
            return Ok(new { Token = "your_generated_token" });
        }
    }
}
