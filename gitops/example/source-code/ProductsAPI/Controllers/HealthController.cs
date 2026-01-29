using Microsoft.AspNetCore.Mvc;
using ProductsAPI.Services;

namespace ProductsAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<HealthController> _logger;

        public HealthController(IProductService productService, ILogger<HealthController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        /// <summary>
        /// Basic health check endpoint
        /// </summary>
        /// <returns>Health status</returns>
        [HttpGet]
        [Route("/health")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Health()
        {
            return Ok(new { 
                status = "Healthy", 
                timestamp = DateTime.UtcNow,
                service = "ProductsAPI",
                version = "1.0.0"
            });
        }

        /// <summary>
        /// Readiness check endpoint
        /// </summary>
        /// <returns>Readiness status</returns>
        [HttpGet]
        [Route("/ready")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
        public async Task<IActionResult> Ready()
        {
            try
            {
                // Check database connectivity by getting product count
                var count = await _productService.GetProductCountAsync();
                
                return Ok(new { 
                    status = "Ready", 
                    timestamp = DateTime.UtcNow,
                    service = "ProductsAPI",
                    database = "Connected",
                    productCount = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Readiness check failed");
                return StatusCode(503, new { 
                    status = "Not Ready", 
                    timestamp = DateTime.UtcNow,
                    service = "ProductsAPI",
                    database = "Disconnected",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Liveness check endpoint
        /// </summary>
        /// <returns>Liveness status</returns>
        [HttpGet]
        [Route("/live")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Live()
        {
            return Ok(new { 
                status = "Alive", 
                timestamp = DateTime.UtcNow,
                service = "ProductsAPI",
                uptime = Environment.TickCount64
            });
        }
    }
}