namespace Backend.Services;

public interface IMinioService
{
    Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken);
    Task UploadAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken);
    Task<string> GetPresignedUrlAsync(string bucketName, string objectName, TimeSpan expiry);
}
