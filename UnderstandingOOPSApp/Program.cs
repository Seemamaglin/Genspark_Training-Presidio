using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Services;

namespace UnderstandingOOPSApp
{
    internal class Program
    {
        ICustomerInteract customerInteract;

        public Program()
        {
            customerInteract = new CustomerService();
        }

        void DoBanking()
        {

            bool runApp=true;

            while (runApp)
            {
                Console.WriteLine("-----Banking Application-----");
                Console.WriteLine("1.Add account");
                Console.WriteLine("2.Print Account details using account number");
                Console.WriteLine("3.Print Account details using phone number");
                Console.WriteLine("4.Exit");
                Console.WriteLine("Please select an option");

                int choice;

                if (!int.TryParse(Console.ReadLine(), out choice))
                {
                    Console.WriteLine("Invalid choice! try again");
                    continue;
                }    

                switch(choice)
                {
                    case 1:
                        var account = customerInteract.OpensAccount();
                        Console.WriteLine("Account created successfully");
                        Console.WriteLine(account);
                        break;

                    case 2:
                        Console.WriteLine("Enter account number:");
                        string accNum = (Console.ReadLine() ?? "").Trim();
                        customerInteract.PrintAccountDetailsbyAccountNumber(accNum);
                        break;

                    case 3:
                        Console.WriteLine("Enter phone number:");
                        string phoneNum = (Console.ReadLine() ?? "").Trim();
                        customerInteract.PrintAccountDetailsByPhone(phoneNum);
                        break;

                    case 4:
                        Console.WriteLine("Exiting application...");
                        runApp = false;
                        break;

                    default:
                        Console.WriteLine("Invalid choice! try again");
                        break;
                } 
            }
        }

        static void Main(string[] args)
        {
            new Program().DoBanking();
        }
    }
}