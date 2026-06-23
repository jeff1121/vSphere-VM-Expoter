using Backend.Models;
using Backend.Services.Stub;

namespace Backend.Tests.Services;

public class VSphereServiceStubTests
{
    private readonly VSphereServiceStub _stub = new();

    [Fact]
    public async Task LoginAsync_ReturnsNonEmptySessionId()
    {
        var request = new LoginRequest { Host = "host", Username = "user", Password = "pass" };

        var sessionId = await _stub.LoginAsync(request, CancellationToken.None);

        Assert.False(string.IsNullOrWhiteSpace(sessionId));
        Assert.True(Guid.TryParse(sessionId, out _));
    }

    [Fact]
    public async Task GetVmsAsync_ReturnsSampleVms()
    {
        var vms = await _stub.GetVmsAsync("any-session", CancellationToken.None);

        Assert.NotNull(vms);
        Assert.NotEmpty(vms);
    }

    [Fact]
    public async Task ExportVmAsync_ReturnsNonEmptyGuid()
    {
        var taskId = await _stub.ExportVmAsync("session", "vm-1", "VMName", CancellationToken.None);

        Assert.NotEqual(Guid.Empty, taskId);
    }

    [Fact]
    public async Task PowerOffAsync_CompletesWithoutException()
    {
        await _stub.PowerOffAsync("session", "vm-1", CancellationToken.None);
        // No exception expected
    }
}
