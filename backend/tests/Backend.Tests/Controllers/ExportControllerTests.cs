using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Controllers;

public class ExportControllerTests
{
    private readonly Mock<IVSphereService> _vsphereServiceMock = new();
    private readonly Mock<IExportTaskStore> _taskStoreMock = new();
    private readonly ExportController _controller;

    public ExportControllerTests()
    {
        _controller = new ExportController(_vsphereServiceMock.Object, _taskStoreMock.Object);
    }

    [Fact]
    public async Task ExportVm_ReturnsBadRequest_WhenSessionIdMissing()
    {
        var result = await _controller.ExportVm("vm-1", null, string.Empty, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ExportVm_ReturnsBadRequest_WhenSessionIdWhitespace()
    {
        var result = await _controller.ExportVm("vm-1", null, "   ", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task ExportVm_ReturnsAccepted_WhenExportStarted()
    {
        var taskId = Guid.NewGuid();
        _vsphereServiceMock
            .Setup(s => s.ExportVmAsync("valid-session", "vm-1", "MyVM", It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskId);

        var result = await _controller.ExportVm("vm-1", "MyVM", "valid-session", CancellationToken.None);

        var accepted = Assert.IsType<AcceptedResult>(result);
        Assert.NotNull(accepted.Value);
        var taskIdProp = accepted.Value!.GetType().GetProperty("taskId");
        Assert.NotNull(taskIdProp);
        Assert.Equal(taskId, taskIdProp.GetValue(accepted.Value));
    }

    [Fact]
    public async Task ExportVm_ReturnsAccepted_WhenVmNameIsNull()
    {
        var taskId = Guid.NewGuid();
        _vsphereServiceMock
            .Setup(s => s.ExportVmAsync("valid-session", "vm-1", "", It.IsAny<CancellationToken>()))
            .ReturnsAsync(taskId);

        var result = await _controller.ExportVm("vm-1", null, "valid-session", CancellationToken.None);

        Assert.IsType<AcceptedResult>(result);
    }

    [Fact]
    public async Task ExportVm_ReturnsUnauthorized_WhenSessionExpired()
    {
        _vsphereServiceMock
            .Setup(s => s.ExportVmAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.ExportVm("vm-1", null, "expired-session", CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }
}
