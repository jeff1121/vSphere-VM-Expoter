using Backend.Models;

namespace Backend.Services.Stub;

public class VSphereServiceStub : IVSphereService
{
    public Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        // Placeholder: real implementation will call vSphere SDK
        return Task.FromResult(Guid.NewGuid().ToString());
    }

    public Task<IReadOnlyCollection<VmInfo>> GetVmsAsync(string sessionId, CancellationToken cancellationToken)
    {
        var random = new Random();
        var sample = new List<VmInfo>
        {
            new() { Id = "vm-1", Name = $"Sample VM ({DateTime.Now:HH:mm:ss})", PowerState = "poweredOn", ProvisionedBytes = 10_000_000_000 },
            new() { Id = "vm-2", Name = "Database VM", PowerState = "poweredOff", ProvisionedBytes = 50_000_000_000 },
            new() { Id = "vm-3", Name = $"Dynamic VM {random.Next(100, 999)}", PowerState = "poweredOn", ProvisionedBytes = 20_000_000_000 },
        };
        return Task.FromResult<IReadOnlyCollection<VmInfo>>(sample);
    }

    public Task<Guid> ExportVmAsync(string sessionId, string vmId, string vmName, CancellationToken cancellationToken)
    {
        return Task.FromResult(Guid.NewGuid());
    }

    public Task PowerOffAsync(string sessionId, string vmId, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
