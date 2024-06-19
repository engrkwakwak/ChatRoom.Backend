namespace Service.Contracts {
    public interface IFileService {
        Task<string> UploadImageAsync(Stream fileStream, string fileName, string contentType, string targetContainer);
    }
}
