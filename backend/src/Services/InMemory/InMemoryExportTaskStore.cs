using Backend.Models;

namespace Backend.Services.InMemory;

public class InMemoryExportTaskStore : IExportTaskStore
{
    private readonly Dictionary<Guid, ExportTask> _tasks = new();
    private readonly object _lock = new();

    public ExportTask Create(string vmId)
    {
        var task = new ExportTask
        {
            Id = Guid.NewGuid(),
            VmId = vmId,
            Status = ExportTaskStatus.Running,
            Progress = 10,
        };

        lock (_lock)
        {
            _tasks[task.Id] = task;
        }

        return task;
    }

    public ExportTask? Get(Guid id)
    {
        lock (_lock)
        {
            return _tasks.TryGetValue(id, out var task) ? task : null;
        }
    }

    public void Update(ExportTask task)
    {
        lock (_lock)
        {
            _tasks[task.Id] = task;
        }
    }

    public IEnumerable<ExportTask> List()
    {
        lock (_lock)
        {
            return _tasks.Values.ToList();
        }
    }
}
