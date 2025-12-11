using Minio;
using Minio.DataModel.Args;

namespace Backend.Services;

public class MinioService : IMinioService
{
    private readonly IMinioClient _minioClient;
    private readonly string _endpoint;
    private readonly string? _publicEndpoint;
    private readonly string _accessKey;
    private readonly string _secretKey;
    private readonly bool _secure;

    public MinioService(IConfiguration configuration)
    {
        _endpoint = configuration["Minio:Endpoint"] ?? "minio:9000";
        _publicEndpoint = configuration["Minio:PublicEndpoint"];
        _accessKey = configuration["Minio:AccessKey"] ?? "minioadmin";
        _secretKey = configuration["Minio:SecretKey"] ?? "minioadmin";
        _secure = bool.Parse(configuration["Minio:Secure"] ?? "false");

        _minioClient = new MinioClient()
            .WithEndpoint(_endpoint)
            .WithCredentials(_accessKey, _secretKey)
            .WithSSL(_secure)
            .Build();
    }

    public async Task EnsureBucketExistsAsync(string bucketName, CancellationToken cancellationToken)
    {
        var beArgs = new BucketExistsArgs().WithBucket(bucketName);
        bool found = await _minioClient.BucketExistsAsync(beArgs, cancellationToken);
        if (!found)
        {
            var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
            await _minioClient.MakeBucketAsync(mbArgs, cancellationToken);
        }
    }

    public async Task UploadAsync(string bucketName, string objectName, Stream data, CancellationToken cancellationToken)
    {
        var putObjectArgs = new PutObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithStreamData(data)
            .WithObjectSize(data.Length)
            .WithContentType("application/x-vmware-ova");
            
        await _minioClient.PutObjectAsync(putObjectArgs, cancellationToken);
    }

    public async Task<string> GetPresignedUrlAsync(string bucketName, string objectName, TimeSpan expiry)
    {
        IMinioClient client = _minioClient;

        // If a public endpoint is configured, use a client configured with it for presigning
        // so the signature matches the Host header the browser will send.
        if (!string.IsNullOrEmpty(_publicEndpoint))
        {
             client = new MinioClient()
                .WithEndpoint(_publicEndpoint)
                .WithCredentials(_accessKey, _secretKey)
                .WithSSL(_secure)
                .Build();
        }

        var args = new PresignedGetObjectArgs()
            .WithBucket(bucketName)
            .WithObject(objectName)
            .WithExpiry((int)expiry.TotalSeconds);

        return await client.PresignedGetObjectAsync(args);
    }
}
