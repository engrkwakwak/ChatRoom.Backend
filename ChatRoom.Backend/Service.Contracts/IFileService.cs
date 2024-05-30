namespace Service.Contracts {
    public interface IFileService {
        Task<string> UploadAsync(Stream fileStream, string fileName, string contentType, int userId);
    }
}
