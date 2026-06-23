using Backend.Models;
using Backend.Services.InMemory;

namespace Backend.Tests.Services;

public class InMemoryExportTaskStoreTests
{
    private readonly InMemoryExportTaskStore _store = new();

    [Fact]
    public void Create_ReturnsNewTask_WithCorrectValues()
    {
        var task = _store.Create("session-1", "vm-1");

        Assert.NotEqual(Guid.Empty, task.Id);
        Assert.Equal("session-1", task.SessionId);
        Assert.Equal("vm-1", task.VmId);
        Assert.Equal(ExportTaskStatus.Running, task.Status);
        Assert.Equal(0, task.Progress);
    }

    [Fact]
    public void Get_ReturnsNull_WhenTaskNotFound()
    {
        var result = _store.Get(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public void Get_ReturnsTask_WhenFound()
    {
        var task = _store.Create("session-1", "vm-1");

        var result = _store.Get(task.Id);

        Assert.NotNull(result);
        Assert.Equal(task.Id, result.Id);
    }

    [Fact]
    public void Update_ModifiesExistingTask()
    {
        var task = _store.Create("session-1", "vm-1");
        task.Status = ExportTaskStatus.Completed;
        task.Progress = 100;
        task.DownloadUrl = "https://example.com/file.ova";

        _store.Update(task);
        var result = _store.Get(task.Id);

        Assert.NotNull(result);
        Assert.Equal(ExportTaskStatus.Completed, result.Status);
        Assert.Equal(100, result.Progress);
        Assert.Equal("https://example.com/file.ova", result.DownloadUrl);
    }

    [Fact]
    public void List_ReturnsAllTasks()
    {
        _store.Create("session-1", "vm-1");
        _store.Create("session-1", "vm-2");
        _store.Create("session-2", "vm-3");

        var list = _store.List().ToList();

        Assert.Equal(3, list.Count);
    }

    [Fact]
    public void List_ReturnsEmptyCollection_WhenNoTasks()
    {
        var list = _store.List().ToList();

        Assert.Empty(list);
    }

    [Fact]
    public void Create_GeneratesUniqueTasks()
    {
        var task1 = _store.Create("session-1", "vm-1");
        var task2 = _store.Create("session-1", "vm-1");

        Assert.NotEqual(task1.Id, task2.Id);
    }

    [Fact]
    public void Update_Task_IsThreadSafe()
    {
        var task = _store.Create("session-1", "vm-1");
        var errors = new System.Collections.Concurrent.ConcurrentBag<Exception>();

        var threads = Enumerable.Range(0, 10).Select(i => new Thread(() =>
        {
            try
            {
                task.Progress = i * 10;
                _store.Update(task);
                _store.Get(task.Id);
            }
            catch (Exception ex)
            {
                errors.Add(ex);
            }
        })).ToList();

        threads.ForEach(t => t.Start());
        threads.ForEach(t => t.Join());

        Assert.Empty(errors);
    }
}
