public class EmailNotification : INotification
{
    public string Message { get; set; }
    public DateTime SentDate { get; set; }

    public EmailNotification(string message)
    {
        Message = message;
        SentDate = DateTime.Now;
    }

    public void Send(User user)
    {
        Console.WriteLine("EMAIL -> " + user.Email);
        Console.WriteLine(Message);
    }
}