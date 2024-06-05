﻿using Azure;
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

        public async Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, int userId) {
            var container = new BlobContainerClient(_azureStorageConnectionString, $"images-{userId}");
            Response<BlobContainerInfo> createResponse = await container.CreateIfNotExistsAsync();

            if (createResponse != null && createResponse.GetRawResponse().Status == 201) 
                await container.SetAccessPolicyAsync(PublicAccessType.Blob);

            BlobClient blob = container.GetBlobClient(AssignPictureName(fileName, "display-picture"));
            await blob.DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots);
            using var imageConvertedStream = await ConvertAndResizeImageToJpg(fileStream);
            await blob.UploadAsync(imageConvertedStream, new BlobHttpHeaders { ContentType = contentType });

            return blob.Uri.ToString();
        }

        private static string AssignPictureName(string currentName, string newName) {
            string extension = Path.GetExtension(currentName);
            return newName + extension;
        }

        private static async Task<MemoryStream> ConvertAndResizeImageToJpg(Stream imageStream) {
            var memoryStream = new MemoryStream();
            if (imageStream.Length < 500 * 1024) {
                await imageStream.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin); // Reset the position to the beginning
                return memoryStream;
            }
            else {
                using var image = await Image.LoadAsync(imageStream);
                image.Mutate(x => x.Resize(width: 500, height: 500, KnownResamplers.Bicubic));
                image.Save(memoryStream, new JpegEncoder { Quality = 100 });
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            
        }
    }
}