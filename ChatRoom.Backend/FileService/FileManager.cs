using Microsoft.Extensions.Configuration;
using Entities.Exceptions;
using Contracts;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using Shared.DataTransferObjects.File;

namespace FileService {
    public class FileManager(IConfiguration configuration, ILoggerManager logger) : IFileManager {
        private readonly string _azureStorageConnectionString = configuration.GetConnectionString("BlobStorageConnection") 
            ?? throw new ConnectionStringNotFoundException("BlobStorageConnection");
        private readonly ILoggerManager _logger = logger;

        public async Task<string> UploadImageAsync(Stream fileStream, PictureForUploadDto picture) {
            var container = new BlobContainerClient(_azureStorageConnectionString, picture.ContainerName!);
            Response<BlobContainerInfo> createResponse = await container.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201)
                await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            BlobClient blob = container.GetBlobClient(GenerateUniqueFileName(picture.FileName!));
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);

            using var resizedStream = await ResizeImageAsync(fileStream);
            await blob.UploadAsync(resizedStream, new BlobHttpHeaders { ContentType = picture.ContentType });
            
            return blob.Uri.ToString();
        }

        public async Task DeleteImageAsync(string blobUri) {
            // Parse the blobUri to get the container name and blob name
            var blobUriBuilder = new BlobUriBuilder(new Uri(blobUri));
            var containerName = blobUriBuilder.BlobContainerName;
            var blobName = blobUriBuilder.BlobName;

            // Create a BlobContainerClient and BlobClient
            var container = new BlobContainerClient(_azureStorageConnectionString, containerName);
            var blobClient = container.GetBlobClient(blobName);

            // Delete the blob
            var response = await blobClient.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            if (!response)
                _logger.LogError($"Blob {blobUri} does not exist or could not be deleted.");
        }

        private static string GenerateUniqueFileName(string originalFileName) {
            string extension = Path.GetExtension(originalFileName);
            return $"{Guid.NewGuid()}{extension}";
        }

        private static Task<Stream> ResizeImageAsync(Stream fileStream) {
            if (fileStream.Length <= 1 * 1024 * 1024) {
                return Task.FromResult(fileStream);
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
