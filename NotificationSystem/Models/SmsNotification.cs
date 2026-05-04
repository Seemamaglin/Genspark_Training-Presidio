public class SmsNotification : INotification
{
    public string Message { get; set; }
    public DateTime SentDate { get; set; }

    public string Status { get; set; }
    public SmsNotification(string message)
    {
        Message = message;
        SentDate = DateTime.Now;
        Status="Pending";
    }

    public void Send(User user)
    {
        Console.WriteLine("SMS: " + user.PhoneNumber);
        Console.WriteLine(Message);
        Console.WriteLine("Sent At: " + SentDate);
        Console.WriteLine("Status: " + Status);
    }
}