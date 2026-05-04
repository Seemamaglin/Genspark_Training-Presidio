using UnderstandingOOPSApp.Models;

namespace UnderstandingOOPSApp.Interfaces
{
    internal interface ICustomerInteract
    {
        Account OpensAccount();
        void PrintAccountDetailsbyAccountNumber(string accountNumber);
        void PrintAccountDetailsByPhone(string phone);
    }
}