public class EmailNotification : INotification
{
    public string Message { get; set; }
    public DateTime SentDate { get; set; }

    public string Status { get; set; }

    public EmailNotification(string message)
    {
        Message = message;
        SentDate = DateTime.Now;
        Status="Pending";
    }

    public void Send(User user)
    {
        Status="Sent";
        Console.WriteLine("EMAIL: " + user.Email);
        Console.WriteLine(Message);
        Console.WriteLine("Sent At: " + SentDate);
        Console.WriteLine("Status: " + Status);
    }
}