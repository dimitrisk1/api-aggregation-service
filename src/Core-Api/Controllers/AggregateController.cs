using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Core_Api.Controllers
{
    [ApiController]
    [Route("api/aggregate")]
    [AllowAnonymous]
    public class AggregateController : ControllerBase
    {
        private readonly IAggregationService _service;

        public AggregateController(IAggregationService service)
        {
            _service = service;
        }

        [HttpGet]
        [ProducesResponseType(typeof(AggregatedResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Get([FromQuery] AggregationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Query))
            {
                return BadRequest(new { error = "Query is required." });
            }

            var result = await _service.AggregateDataAsync(request, HttpContext.RequestAborted);
            return Ok(result);
        }
    }
}
