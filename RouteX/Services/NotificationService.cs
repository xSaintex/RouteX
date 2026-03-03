namespace RouteX.Services
{
    public interface INotificationService
    {
        void AddSuccess(string message);
        void AddError(string message);
        void AddWarning(string message);
        void AddInfo(string message);
        void ClearNotifications();
        List<Notification> GetNotifications();
    }

    public class Notification
    {
        public string Message { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.Now;
    }

    public enum NotificationType
    {
        Success,
        Error,
        Warning,
        Info
    }

    public class NotificationService : INotificationService
    {
        private readonly List<Notification> _notifications = new List<Notification>();

        public void AddSuccess(string message)
        {
            _notifications.Add(new Notification { Message = message, Type = NotificationType.Success });
        }

        public void AddError(string message)
        {
            _notifications.Add(new Notification { Message = message, Type = NotificationType.Error });
        }

        public void AddWarning(string message)
        {
            _notifications.Add(new Notification { Message = message, Type = NotificationType.Warning });
        }

        public void AddInfo(string message)
        {
            _notifications.Add(new Notification { Message = message, Type = NotificationType.Info });
        }

        public void ClearNotifications()
        {
            _notifications.Clear();
        }

        public List<Notification> GetNotifications()
        {
            return _notifications.ToList();
        }
    }
}
