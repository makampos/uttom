using Minio;
using Minio.DataModel.Args;
using Uttom.Domain.Interfaces.Services;

namespace Uttom.Infrastructure.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly string _bucketName = "deliverer-images"; // Bucket name

        public MinioService()
        {
            var endpoint = "localhost:9000";
            var accessKey = "minioadmin";
            var secretKey = "minioadmin";

            _minioClient = new MinioClient()
                .WithEndpoint(endpoint)
                .WithCredentials(accessKey, secretKey)
                .Build();

            // Ensure the bucket exists on initialization
            CreateBucketIfNotExists().Wait();
        }

        public async Task<string> UploadImageAsync(int delivererId, string base64ImageData)
        {
            if (string.IsNullOrWhiteSpace(base64ImageData))
            {
                throw new ArgumentException("The base64 image data cannot be null or empty.", nameof(base64ImageData));
            }

            // Remove the data URL scheme if present (e.g., "data:image/jpeg;base64,"), leaving only the Base64 data
            var base64Data = base64ImageData.Contains(",")
                ? base64ImageData.Split(',')[1]
                : base64ImageData;

            var imageData = Convert.FromBase64String(base64Data);

            var objectName = $"{delivererId}-{Guid.NewGuid()}.png"; //TODO: remove hard-coded extension
            using (var stream = new MemoryStream(imageData))
            {
                var args = new PutObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType("image/png"); // TODO: ADD Extension

                await _minioClient.PutObjectAsync(args);
            }

            return objectName; // Return stored object name
        }

        public async Task<MemoryStream> GetImageAsync(string objectName)
        {
            var memoryStream = new MemoryStream();

            // Retrieve the object directly into the MemoryStream
            await _minioClient.GetObjectAsync(
                new GetObjectArgs()
                    .WithBucket(_bucketName)
                    .WithObject(objectName)
                    .WithCallbackStream((stream) => stream.CopyTo(memoryStream))
            );

            // Reset the position of the MemoryStream before returning it
            memoryStream.Position = 0;

            return memoryStream; // Return the MemoryStream containing the image data
        }

        private async Task CreateBucketIfNotExists()
        {
            var bucketExistsArgs = new BucketExistsArgs()
                .WithBucket(_bucketName);

            var found = await _minioClient.BucketExistsAsync(bucketExistsArgs);

            if (!found)
            {
                var makeBucketArgs = new MakeBucketArgs()
                    .WithBucket(_bucketName)
                    .WithLocation("us-east-1");

                // Create the bucket
                await _minioClient.MakeBucketAsync(makeBucketArgs);
            }
        }
    }
}