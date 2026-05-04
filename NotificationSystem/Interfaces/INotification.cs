public interface INotification
{
    string Message { get; set; }
    DateTime SentDate { get; set; }
    void Send(User user);
}