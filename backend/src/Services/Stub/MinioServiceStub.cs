namespace Backend.Services.Stub;

public class MinioServiceStub : IMinioService
{
    public Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        // Placeholder stub
        return Task.CompletedTask;
    }

    public Task UploadAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken)
    {
        // Placeholder stub
        return Task.CompletedTask;
    }

    public Task<string> GetPresignedUrlAsync(string bucketName, string objectName, TimeSpan expiry)
    {
        // Placeholder stub URL
        return Task.FromResult($"https://example.com/{bucketName}/{objectName}");
    }
}
