using Microsoft.AspNetCore.Mvc;

namespace ProjectS.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly ILogger<HealthController> _logger;

        public HealthController(ILogger<HealthController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("health")]
        public async Task GetGealth()
        {
            _logger.LogInformation("ОЛШОЛООЛОЛОЛОЛОЛОЛОЛОЛ");
        }
    }
}
