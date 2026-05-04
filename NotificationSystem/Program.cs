class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Notification System");

        Console.Write("Enter Name: ");
        string name = Console.ReadLine() ?? "";

        Console.Write("Enter Email: ");
        string email = Console.ReadLine() ?? "";

        Console.Write("Enter Phone Number: ");
        string phone = Console.ReadLine() ?? "";

        User user = new User(name, email, phone);

        NotificationService service = new NotificationService();

        while (true)
        {
            Console.WriteLine("\nChoose Notification Type:");
            Console.WriteLine("1. Email");
            Console.WriteLine("2. SMS");
            Console.WriteLine("3. Exit");
            Console.Write("Enter choice: ");

            int choice = int.Parse(Console.ReadLine() ?? "0");

            if (choice == 3)
            {
                Console.WriteLine("Exiting... Bye!");
                break;
            }

            Console.Write("Enter Message: ");
            string message = Console.ReadLine() ?? "";

            if (choice == 1)
            {
                INotification emailNotification = new EmailNotification(message);
                service.SendNotification(emailNotification, user);
            }
            else if (choice == 2)
            {
                INotification smsNotification = new SmsNotification(message);
                service.SendNotification(smsNotification, user);
            }
            else
            {
                Console.WriteLine("Invalid choice!");
            }
        }
    }
}