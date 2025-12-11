using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VmsController : ControllerBase
{
    private readonly IVSphereService _vsphereService;

    public VmsController(IVSphereService vsphereService)
    {
        _vsphereService = vsphereService;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromHeader(Name = "X-Session-Id")] string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest("缺少 Session Id");
        }

        try
        {
            var vms = await _vsphereService.GetVmsAsync(sessionId, cancellationToken);
            return Ok(vms);
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Session 已過期或無效");
        }
    }
}
