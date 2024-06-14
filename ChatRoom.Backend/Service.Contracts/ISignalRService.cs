namespace Service.Contracts {
    public interface ISignalRService {
        int GetUserIdFromToken(string token);
    }
}
