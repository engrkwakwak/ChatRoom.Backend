using Shared.DataTransferObjects.File;

namespace Contracts {
    public interface IFileManager {
        Task<string> UploadImageAsync(Stream fileStream, PictureForUploadDto picture);
        Task DeleteImageAsync(string blobUri);
    }
}
