using Microsoft.AspNetCore.Mvc;

namespace HomeOS.Api.Controllers;

[ApiController]
[Route("api/status")]
public class StatusController(IConfiguration configuration) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new
    {
        service = "Home OS API",
        status = "ok",
        utc = DateTime.UtcNow,
        databaseConfigured = !string.IsNullOrWhiteSpace(configuration.GetConnectionString("HomeOsDb"))
    });
}
