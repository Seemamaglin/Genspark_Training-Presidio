public class NotificationService
{
    public void SendNotification(INotification notification, User user)
    {
        notification.Send(user);
    }
}