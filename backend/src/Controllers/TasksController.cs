using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;

namespace Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly IExportTaskStore _taskStore;

    public TasksController(IExportTaskStore taskStore)
    {
        _taskStore = taskStore;
    }

    [HttpGet("{taskId:guid}")]
    public ActionResult<ExportTask> GetTask(Guid taskId)
    {
        var task = _taskStore.Get(taskId);
        return task is null ? NotFound() : Ok(task);
    }
}
