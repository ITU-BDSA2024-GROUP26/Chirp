#if DEBUG
using Infrastructure;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers;

[ApiController]
[Route("api/development")]
public class DevelopmentController(ICheepService service) : ControllerBase
{
    [HttpPost("reset")]
    public async Task<IActionResult> ResetDatabase()
    {
        await service.ResetDatabaseAsync();
        return Ok("Database reset successfully.");
    }

    [HttpPost("seed")]
    public async Task<IActionResult> SeedDatabase()
    {
        await service.SeedDatabaseAsync();
        return Ok("Database seeded successfully.");
    }
}
#endif