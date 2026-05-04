public class SmsNotification : INotification
{
    public string Message { get; set; }
    public DateTime SentDate { get; set; }

    public SmsNotification(string message)
    {
        Message = message;
        SentDate = DateTime.Now;
    }

    public void Send(User user)
    {
        Console.WriteLine("SMS -> " + user.PhoneNumber);
        Console.WriteLine(Message);
    }
}