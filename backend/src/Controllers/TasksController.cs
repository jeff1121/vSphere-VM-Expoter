using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IExportTaskStore _taskStore;
    private readonly ISessionStore _sessionStore;

    public TasksController(IExportTaskStore taskStore, ISessionStore sessionStore)
    {
        _taskStore = taskStore;
        _sessionStore = sessionStore;
    }

    [HttpGet("{taskId:guid}")]
    public ActionResult<ExportTask> GetTask(Guid taskId, [FromHeader(Name = "X-Session-Id")] string sessionId)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return BadRequest("缺少 Session Id");
        }

        if (_sessionStore.Get(sessionId) is null)
        {
            return Unauthorized("Session 已過期或無效");
        }

        var task = _taskStore.Get(taskId);
        if (task is null || !string.Equals(task.SessionId, sessionId, StringComparison.Ordinal))
        {
            return NotFound();
        }

        return Ok(task);
    }
}
