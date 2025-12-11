using Backend.Models;

namespace Backend.Services;

public interface IVSphereService
{
    Task<string> LoginAsync(LoginRequest request, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<VmInfo>> GetVmsAsync(string sessionId, CancellationToken cancellationToken);
    Task<Guid> ExportVmAsync(string sessionId, string vmId, CancellationToken cancellationToken);
}
