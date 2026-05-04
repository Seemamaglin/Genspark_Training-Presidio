using System;
using System.Collections.Generic;
using UnderstandingOOPSApp.Interfaces;
using UnderstandingOOPSApp.Models;

namespace UnderstandingOOPSApp.Services
{
    internal class CustomerService : ICustomerInteract
    {
        static List<Account> accounts = new List<Account>();
        static string lastAccountNumber = "9990001000";

        public Account OpensAccount()
        {
            Account account = TakeCustomerDetails();
            TakeInitialDeposit(account);

            long accNum = Convert.ToInt64(lastAccountNumber);
            account.AccountNumber = (++accNum).ToString();
            lastAccountNumber = accNum.ToString();

            accounts.Add(account);
            return account;
        }

        private void TakeInitialDeposit(Account account)
        {
            Console.WriteLine("Do you want to deposit any amount now. If yes enter the amount else enter 0");
            float amount;
            while (!float.TryParse(Console.ReadLine(), out amount))
                Console.WriteLine("Invalid entry. Please enter the deposit amount");

            account.Balance += amount;
        }

        private Account TakeCustomerDetails()
        {
            Account account;

            Console.WriteLine("Select account type: 1 for Savings, 2 for Current");
            int typeChoice;
            while (!int.TryParse(Console.ReadLine(), out typeChoice) || typeChoice < 1 || typeChoice > 2)
                Console.WriteLine("Invalid entry. Try again");

            account = typeChoice == 1 ? new SavingAccount() : new CurrentAccount();

            Console.WriteLine("Enter full name");
            account.NameOnAccount = (Console.ReadLine() ?? "").Trim();

            Console.WriteLine("Enter DOB (yyyy-mm-dd)");
            DateTime dob;
            while (!DateTime.TryParse(Console.ReadLine(), out dob) || dob > DateTime.Today)
                Console.WriteLine("Invalid date");

            account.DateOfBirth = dob;

            Console.WriteLine("Enter email");
            account.Email = (Console.ReadLine() ?? "").Trim().ToLower();

            Console.WriteLine("Enter phone");
            account.Phone = (Console.ReadLine() ?? "").Trim();

            return account;
        }

        public void PrintAccountDetailsbyAccountNumber(string accountNumber)
        {
            accountNumber = accountNumber.Trim();

            Account account = accounts.Find(
                a => a.AccountNumber.Trim() == accountNumber
            );

            if (account == null)
            {
                Console.WriteLine("No account found");
                return;
            }

            PrintAccount(account);
        }

        public void PrintAccountDetailsByPhone(string phone)
        {
            phone = phone.Trim();

            Account account = accounts.Find(
                a => a.Phone.Trim() == phone
            );

            if (account == null)
            {
                Console.WriteLine("No account found with this phone number");
                return;
            }

            PrintAccount(account);
        }

        private void PrintAccount(Account account)
        {
            Console.WriteLine("-----------------------------");
            Console.WriteLine(account);
            Console.WriteLine("-----------------------------");
        }
    }
}