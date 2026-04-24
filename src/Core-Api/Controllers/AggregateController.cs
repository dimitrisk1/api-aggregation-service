namespace Core_Api.Controllers
{
    using Application.DTOs;
    using Application.Interfaces;
    using Microsoft.AspNetCore.Mvc;

    [Route("[controller]")]
    [ApiController]
    public class AggregateController : ControllerBase
    {
        private readonly IAggregationService _service;

        public AggregateController(IAggregationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> Get([FromQuery] AggregationRequest request)
        {
            var result = await _service.HandleAsync(request);
            return Ok(result);
        }
    }
}
