using Backend.Models;

namespace Backend.Services;

public interface IExportTaskStore
{
    ExportTask Create(string sessionId, string vmId);
    ExportTask? Get(Guid id);
    void Update(ExportTask task);
    IEnumerable<ExportTask> List();
}
