using Backend.Services.Stub;

namespace Backend.Tests.Services;

public class MinioServiceStubTests
{
    private readonly MinioServiceStub _stub = new();

    [Fact]
    public async Task EnsureBucketExistsAsync_CompletesWithoutException()
    {
        await _stub.EnsureBucketExistsAsync("test-bucket", CancellationToken.None);
        // No exception expected
    }

    [Fact]
    public async Task UploadAsync_CompletesWithoutException()
    {
        using var stream = new MemoryStream(new byte[] { 1, 2, 3 });

        await _stub.UploadAsync("test-bucket", "test.ova", stream, CancellationToken.None);
        // No exception expected
    }

    [Fact]
    public async Task GetPresignedUrlAsync_ReturnsUrlContainingBucketAndObject()
    {
        var url = await _stub.GetPresignedUrlAsync("mybucket", "myfile.ova", TimeSpan.FromHours(1));

        Assert.Contains("mybucket", url);
        Assert.Contains("myfile.ova", url);
    }
}
