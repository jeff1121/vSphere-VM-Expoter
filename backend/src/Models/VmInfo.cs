namespace Backend.Models;

public class VmInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string PowerState { get; set; } = string.Empty;
    public long? ProvisionedBytes { get; set; }
}
