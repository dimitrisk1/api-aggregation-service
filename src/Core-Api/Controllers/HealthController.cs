using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Core_Api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            return Ok("The app is running.");
        }
    }
}
