namespace Shared.Utilities {
    public static class EnumHelper {
        public enum ChatTypes {
            P2P = 1,
            GroupChat = 2
        }

        public enum MessageTypes {
            Normal = 1,
            Notification = 2
        }

        public enum Status {
            Active = 1,
            Approved = 2,
            Deleted = 3
        }
    }
}
