namespace Core_Api.Controllers
{
    using Application.Interfaces;
    using Microsoft.AspNetCore.Mvc;
    [Route("[controller]")]
    [ApiController]
    public class StatsController : ControllerBase
    {
        private readonly IApiMetricsService _metrics;

        public StatsController(IApiMetricsService metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok(_metrics.GetStats());
        }
    }
}
