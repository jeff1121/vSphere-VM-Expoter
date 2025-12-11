namespace Backend.Models;

public enum ExportTaskStatus
{
    Pending,
    Running,
    Completed,
    Failed
}

public class ExportTask
{
    public Guid Id { get; set; }
    public string VmId { get; set; } = string.Empty;
    public ExportTaskStatus Status { get; set; } = ExportTaskStatus.Pending;
    public int Progress { get; set; }
    public string? DownloadUrl { get; set; }
    public string? Error { get; set; }
}
