using Backend.Controllers;
using Backend.Models;
using Backend.Services;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace Backend.Tests.Controllers;

public class AuthControllerTests
{
    private readonly Mock<IVSphereService> _vsphereServiceMock = new();
    private readonly AuthController _controller;

    public AuthControllerTests()
    {
        _controller = new AuthController(_vsphereServiceMock.Object);
    }

    [Theory]
    [InlineData("", "user")]
    [InlineData("  ", "user")]
    [InlineData("host", "")]
    [InlineData("host", "  ")]
    public async Task Login_ReturnsBadRequest_WhenHostOrUsernameEmpty(string host, string username)
    {
        var request = new LoginRequest { Host = host, Username = username, Password = "pass" };

        var result = await _controller.Login(request, CancellationToken.None);

        var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(bad.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_ReturnsOk_WhenLoginSucceeds()
    {
        var sessionId = Guid.NewGuid().ToString();
        _vsphereServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(sessionId);

        var request = new LoginRequest { Host = "vcenter.local", Username = "admin", Password = "secret" };

        var result = await _controller.Login(request, CancellationToken.None);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(ok.Value);
        Assert.True(response.Success);
        Assert.Equal(sessionId, response.SessionId);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenHttpRequestExceptionIsUnauthorized()
    {
        _vsphereServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Unauthorized", null, System.Net.HttpStatusCode.Unauthorized));

        var request = new LoginRequest { Host = "vcenter.local", Username = "admin", Password = "wrong" };

        var result = await _controller.Login(request, CancellationToken.None);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        var response = Assert.IsType<LoginResponse>(unauthorized.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_Returns500_WhenHttpRequestExceptionIsNotUnauthorized()
    {
        _vsphereServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection refused", null, System.Net.HttpStatusCode.ServiceUnavailable));

        var request = new LoginRequest { Host = "vcenter.local", Username = "admin", Password = "secret" };

        var result = await _controller.Login(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        var response = Assert.IsType<LoginResponse>(objectResult.Value);
        Assert.False(response.Success);
    }

    [Fact]
    public async Task Login_Returns500_WhenGeneralExceptionThrown()
    {
        _vsphereServiceMock
            .Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Unexpected error"));

        var request = new LoginRequest { Host = "vcenter.local", Username = "admin", Password = "secret" };

        var result = await _controller.Login(request, CancellationToken.None);

        var objectResult = Assert.IsType<ObjectResult>(result.Result);
        Assert.Equal(500, objectResult.StatusCode);
        var response = Assert.IsType<LoginResponse>(objectResult.Value);
        Assert.False(response.Success);
        Assert.Contains("Unexpected error", response.Message);
    }
}
