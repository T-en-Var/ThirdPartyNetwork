using Microsoft.AspNetCore.Mvc;

namespace thirdpartynetwork.Presentation.Controllers;

[Route("api/health")]
[ApiController]
public class HealthController() : ControllerBase
{
    [HttpGet("")]
    public IActionResult Health()
    {
        return Ok(new { status = "Healthy", message = "The API is running" });
    }
}
