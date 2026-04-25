using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_Api.Controllers
{
    [ApiController]
    [Route("api/stats")]
    [AllowAnonymous]
    public class StatsController : ControllerBase
    {
        private readonly IApiMetricsService _metrics;

        public StatsController(IApiMetricsService metrics)
        {
            _metrics = metrics;
        }

        [HttpGet]
        [ProducesResponseType(typeof(ApiStatsDto), StatusCodes.Status200OK)]
        public IActionResult Get()
        {
            return Ok(_metrics.GetApiStatistics());
        }
    }
}
