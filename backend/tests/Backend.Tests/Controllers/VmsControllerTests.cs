using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Controllers;

public class VmsControllerTests
{
    private readonly Mock<IVSphereService> _vsphereServiceMock = new();
    private readonly VmsController _controller;

    public VmsControllerTests()
    {
        _controller = new VmsController(_vsphereServiceMock.Object);
    }

    [Fact]
    public async Task Get_ReturnsBadRequest_WhenSessionIdMissing()
    {
        var result = await _controller.Get(string.Empty, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsBadRequest_WhenSessionIdWhitespace()
    {
        var result = await _controller.Get("   ", CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Get_ReturnsOk_WithVmList()
    {
        var vms = new List<VmInfo>
        {
            new() { Id = "vm-1", Name = "Test VM", PowerState = "POWERED_ON", ProvisionedBytes = 1000 }
        };
        _vsphereServiceMock
            .Setup(s => s.GetVmsAsync("valid-session", It.IsAny<CancellationToken>()))
            .ReturnsAsync(vms);

        var result = await _controller.Get("valid-session", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Equal(vms, ok.Value);
    }

    [Fact]
    public async Task Get_ReturnsUnauthorized_WhenSessionExpired()
    {
        _vsphereServiceMock
            .Setup(s => s.GetVmsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.Get("expired-session", CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task PowerOff_ReturnsBadRequest_WhenSessionIdMissing()
    {
        var result = await _controller.PowerOff("vm-1", string.Empty, CancellationToken.None);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task PowerOff_ReturnsOk_WhenSucceeds()
    {
        _vsphereServiceMock
            .Setup(s => s.PowerOffAsync("valid-session", "vm-1", It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var result = await _controller.PowerOff("vm-1", "valid-session", CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task PowerOff_ReturnsUnauthorized_WhenSessionExpired()
    {
        _vsphereServiceMock
            .Setup(s => s.PowerOffAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException());

        var result = await _controller.PowerOff("vm-1", "expired-session", CancellationToken.None);

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    [Fact]
    public async Task PowerOff_ReturnsBadRequest_WhenInvalidOperation()
    {
        _vsphereServiceMock
            .Setup(s => s.PowerOffAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("VM is already off"));

        var result = await _controller.PowerOff("vm-1", "valid-session", CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("VM is already off", bad.Value);
    }
}
