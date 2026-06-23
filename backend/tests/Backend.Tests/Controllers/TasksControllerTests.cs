using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Controllers;

public class TasksControllerTests
{
    private readonly Mock<IExportTaskStore> _taskStoreMock = new();
    private readonly Mock<ISessionStore> _sessionStoreMock = new();
    private readonly TasksController _controller;

    public TasksControllerTests()
    {
        _controller = new TasksController(_taskStoreMock.Object, _sessionStoreMock.Object);
    }

    [Fact]
    public void GetTask_ReturnsBadRequest_WhenSessionIdMissing()
    {
        var result = _controller.GetTask(Guid.NewGuid(), string.Empty);

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetTask_ReturnsBadRequest_WhenSessionIdWhitespace()
    {
        var result = _controller.GetTask(Guid.NewGuid(), "   ");

        Assert.IsType<BadRequestObjectResult>(result.Result);
    }

    [Fact]
    public void GetTask_ReturnsUnauthorized_WhenSessionNotFound()
    {
        _sessionStoreMock
            .Setup(s => s.Get("invalid-session"))
            .Returns((SessionData?)null);

        var result = _controller.GetTask(Guid.NewGuid(), "invalid-session");

        Assert.IsType<UnauthorizedObjectResult>(result.Result);
    }

    [Fact]
    public void GetTask_ReturnsNotFound_WhenTaskNotFound()
    {
        var sessionId = "valid-session";
        _sessionStoreMock
            .Setup(s => s.Get(sessionId))
            .Returns(new SessionData("host", "token", "user", "pass"));
        _taskStoreMock
            .Setup(s => s.Get(It.IsAny<Guid>()))
            .Returns((ExportTask?)null);

        var result = _controller.GetTask(Guid.NewGuid(), sessionId);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetTask_ReturnsNotFound_WhenTaskBelongsToDifferentSession()
    {
        var sessionId = "valid-session";
        var taskId = Guid.NewGuid();
        _sessionStoreMock
            .Setup(s => s.Get(sessionId))
            .Returns(new SessionData("host", "token", "user", "pass"));
        _taskStoreMock
            .Setup(s => s.Get(taskId))
            .Returns(new ExportTask { Id = taskId, SessionId = "other-session", VmId = "vm-1" });

        var result = _controller.GetTask(taskId, sessionId);

        Assert.IsType<NotFoundResult>(result.Result);
    }

    [Fact]
    public void GetTask_ReturnsOk_WhenTaskFound()
    {
        var sessionId = "valid-session";
        var taskId = Guid.NewGuid();
        var task = new ExportTask
        {
            Id = taskId,
            SessionId = sessionId,
            VmId = "vm-1",
            Status = ExportTaskStatus.Completed,
            Progress = 100
        };
        _sessionStoreMock
            .Setup(s => s.Get(sessionId))
            .Returns(new SessionData("host", "token", "user", "pass"));
        _taskStoreMock
            .Setup(s => s.Get(taskId))
            .Returns(task);

        var result = _controller.GetTask(taskId, sessionId);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(task, ok.Value);
    }
}
