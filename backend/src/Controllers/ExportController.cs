using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExportController : ControllerBase
{
    private readonly IVSphereService _vsphereService;
    private readonly IExportTaskStore _taskStore;

    public ExportController(IVSphereService vsphereService, IExportTaskStore taskStore)
    {
        _vsphereService = vsphereService;
        _taskStore = taskStore;
    }

    [HttpPost("{vmId}")]
    public async Task<IActionResult> ExportVm(string vmId, [FromQuery] string? vmName, [FromHeader(Name = "X-Session-Id")] string sessionId, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest("缺少 Session Id");
        }

        try
        {
            var taskId = await _vsphereService.ExportVmAsync(sessionId, vmId, vmName ?? "", cancellationToken);
            return Accepted(new { taskId });
        }
        catch (UnauthorizedAccessException)
        {
            return Unauthorized("Session 已過期或無效");
        }
    }
}
