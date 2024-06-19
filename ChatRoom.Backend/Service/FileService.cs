using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Entities.Exceptions;
using Microsoft.Extensions.Configuration;
using Service.Contracts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;

namespace Service {
    internal sealed class FileService(IConfiguration configuration) : IFileService {
        private readonly string _azureStorageConnectionString = configuration.GetConnectionString("AzuriteStorageConnection") ?? throw new ConnectionStringNotFoundException("AzuriteStorageConnection");

        public async Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string targetContainer) {
            var container = new BlobContainerClient(_azureStorageConnectionString, targetContainer);
            Response<BlobContainerInfo> createResponse = await container.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            BlobClient blob = container.GetBlobClient(GenerateUniqueFileName(fileName));
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            using var resizedStream = await ResizeImageAsync(fileStream);
            await blob.UploadAsync(resizedStream, new BlobHttpHeaders { ContentType = contentType });

            return blob.Uri.ToString();
        }

        private static string GenerateUniqueFileName(string originalFileName) {
            string extension = Path.GetExtension(originalFileName);
            return $"{Guid.NewGuid()}{extension}";
        }

        private static Task<Stream> ResizeImageAsync(Stream fileStream) {
            if (fileStream.Length <= 1 * 1024 * 1024) {
                return Task.FromResult<Stream>(fileStream);
            }

            using var image = Image.Load(fileStream);

            var options = new ResizeOptions {
                Mode = ResizeMode.Max,
                Size = new Size(1920, 1080)
            };

            image.Mutate(x => x.Resize(options));

            var encoder = new JpegEncoder() {
                Quality = 80
            };

            var resizedStream = new MemoryStream();
            image.Save(resizedStream, encoder);

            resizedStream.Seek(0, SeekOrigin.Begin);
            return Task.FromResult<Stream>(resizedStream);
        }
    }
}
