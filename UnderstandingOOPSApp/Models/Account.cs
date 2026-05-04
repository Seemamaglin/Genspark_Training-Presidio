using System;

namespace UnderstandingOOPSApp.Models
{
    internal class Account
    {
        public string AccountNumber { get; set; } = string.Empty;
        public string NameOnAccount { get; set; } = string.Empty;
        public DateTime DateOfBirth { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public float Balance { get; set; }
        public AccType AccountType { get; set; }

        public Account() { }

        public Account(string accountNumber, string nameOnAccount,
                       DateTime dateOfBirth, string email,
                       string phone, float balance)
        {
            AccountNumber = accountNumber;
            NameOnAccount = nameOnAccount;
            DateOfBirth = dateOfBirth;
            Email = email;
            Phone = phone;
            Balance = balance;
        }

        public override string ToString()
        {
            return $"Account Number : {AccountNumber}\n" +
                   $"Account Type : {AccountType}\n" +
                   $"Account Holder Name : {NameOnAccount}\n" +
                   $"Phone Number : {Phone}\n" +
                   $"Email : {Email}\n" +
                   $"Balance : {Balance}";
        }
    }
}
