using System;

namespace ATMclass
{
    public class Account
    {
        public string CardNumber { get; set; }
        public string OwnerName { get; set; }
        public decimal Balance { get; set; }
        public string PIN { get; set; }
        public string BankName { get; set; }


        public Account(string cardNumber, string ownerName, string pin, decimal balance, string bankName)
        {
            CardNumber = cardNumber;
            OwnerName = ownerName;
            PIN = pin;
            Balance = balance;
            BankName = bankName;
        }
    }

    public class AutomatedTellerMachine
    {
        public string ATMIdentifier { get; set; }
        public decimal Cash { get; set; }
        public string Location { get; set; }

        public AutomatedTellerMachine(string atmIdentifier, decimal cash, string location)
        {
            ATMIdentifier = atmIdentifier;
            Cash = cash;
            Location = location;
        }
    }

    public class Bank
    {
        public string BankName { get; set; }
        public AutomatedTellerMachine[] ATMs { get; set; }

        public Bank(string bankName, AutomatedTellerMachine[] atms)
        {
            BankName = bankName;
            ATMs = atms;
        }
    }
}
