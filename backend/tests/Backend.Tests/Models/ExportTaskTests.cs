using Backend.Models;

namespace Backend.Tests.Models;

public class ExportTaskTests
{
    [Fact]
    public void ExportTask_DefaultValues_AreCorrect()
    {
        var task = new ExportTask();

        Assert.Equal(Guid.Empty, task.Id);
        Assert.Equal(string.Empty, task.VmId);
        Assert.Equal(string.Empty, task.SessionId);
        Assert.Equal(ExportTaskStatus.Pending, task.Status);
        Assert.Equal(0, task.Progress);
        Assert.Null(task.DownloadUrl);
        Assert.Null(task.Error);
    }

    [Fact]
    public void ExportTaskStatus_HasExpectedValues()
    {
        Assert.Equal(0, (int)ExportTaskStatus.Pending);
        Assert.Equal(1, (int)ExportTaskStatus.Running);
        Assert.Equal(2, (int)ExportTaskStatus.Completed);
        Assert.Equal(3, (int)ExportTaskStatus.Failed);
    }
}
