using ATMclass;
using System;
using System.Data.SqlClient;
using System.Security.Cryptography;
using System.Text;


namespace ATMConsole
{
    public static class PasswordHasher
    {
        public static string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
                byte[] hashBytes = sha256.ComputeHash(passwordBytes);
                StringBuilder hashStringBuilder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    hashStringBuilder.Append(b.ToString("x2"));
                }
                return hashStringBuilder.ToString();
            }
        }
    }
    class Program
    {
        public delegate void ATMEventHandler(string message);
        public static event ATMEventHandler OnAuthenticate;
        public static event ATMEventHandler OnWithdraw;
        public static event ATMEventHandler OnDeposit;
        public static event ATMEventHandler OnTransfer;

        static string connectionString = @"Server=DESKTOP-I41K1CO\SQLEXPRESS;Database=ATM_DB;Trusted_Connection=True;";

        static void Main(string[] args)
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
            OnAuthenticate += MessageHandler;
            OnWithdraw += MessageHandler;
            OnDeposit += MessageHandler;
            OnTransfer += MessageHandler;

            Account account = null;

            while (account == null)
            {
                Console.WriteLine("Введіть номер картки (4-значне число):");
                string cardNumber = Console.ReadLine();

                if (!IsValidCardNumber(cardNumber))
                {
                    Console.WriteLine("Неправильне введення! Номер картки має бути 4-значним числом.");
                    continue;
                }

                Console.WriteLine("Введіть PIN (4-значне число):");
                string pin = Console.ReadLine();

                if (!IsValidPIN(pin))
                {
                    Console.WriteLine("Неправильне введення! PIN має бути 4-значним числом.");
                    continue;
                }

                account = Authenticate(cardNumber, pin);

                if (account == null)
                {
                    Console.WriteLine("Невірний номер картки або PIN.");
                }
                else
                {
                    OnAuthenticate?.Invoke($"Успішна аутентифікація.\nІм'я власника: {account.OwnerName}\nБанк: {account.BankName}");
                    MainMenu(account);
                }
            }
        }

        static bool IsValidCardNumber(string cardNumber)
        {
            return cardNumber.Length == 4 && int.TryParse(cardNumber, out _);
        }

        static bool IsValidPIN(string pin)
        {
            return pin.Length == 4 && int.TryParse(pin, out _);
        }


        static Account Authenticate(string cardNumber, string pin)
        {
            string hashedPin = PasswordHasher.HashPassword(pin);
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = @"SELECT a.CardNumber, a.OwnerName, a.PIN, a.Balance, b.BankName
                         FROM Accounts a
                         JOIN Banks b ON a.BankID = b.BankID
                         WHERE a.CardNumber = @CardNumber AND a.PIN = @PIN";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CardNumber", cardNumber);
                command.Parameters.AddWithValue("@PIN", hashedPin);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return new Account(
                        reader["CardNumber"].ToString(),
                        reader["OwnerName"].ToString(),
                        reader["PIN"].ToString(),
                        (decimal)reader["Balance"],
                        reader["BankName"].ToString()
                    );
                }

                return null;
            }
        }


        static void MainMenu(Account account)
        {
            bool exit = false;
            while (!exit)
            {
                
                //Console.Clear();
                Console.WriteLine("\nОберіть операцію:");
                Console.WriteLine("1. Переглянути баланс");
                Console.WriteLine("2. Зняти кошти");
                Console.WriteLine("3. Зарахувати кошти");
                Console.WriteLine("4. Перерахувати кошти");
                Console.WriteLine("5. Вийти");

                switch (Console.ReadLine())
                {
                    case "1":
                        Console.Clear();
                        ShowBalance(account);
                        break;
                    case "2":
                        Withdraw(account);
                        break;
                    case "3":
                        Deposit(account);
                        break;
                    case "4":
                        Transfer(account);
                        break;
                    case "5":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Невірний вибір.");
                        break;
                }
            }
        }

        static void ShowBalance(Account account)
        {
            Console.WriteLine($"Ваш баланс: {account.Balance} грн");
        }

        static void Withdraw(Account account)
        {
            decimal amount = 0;
            bool validAmount = false;

            while (!validAmount)
            {
                Console.Clear();
                Console.WriteLine("Введіть суму для зняття (ціле або дробове число) або 'exit' для виходу:");

                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Операція скасована.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                if (decimal.TryParse(input, out amount) && amount > 0)
                {
                    validAmount = true;
                }
                else
                {
                    Console.WriteLine("Неправильне введення! Сума має бути числом більше нуля.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();

                }
            }

            if (account.Balance >= amount)
            {
                account.Balance -= amount;
                UpdateBalance(account);
                OnWithdraw?.Invoke($"Успішно знято {amount} грн");
                LogTransaction(account.CardNumber, amount, "Withdrawal");
                Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                Console.ReadKey();
                Console.Clear();
            }
            else
            {
                Console.WriteLine("Недостатньо коштів.");
                Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                Console.ReadKey();

            }
        }


        static void Deposit(Account account)
        {
            decimal amount = 0;
            bool validAmount = false;

            while (!validAmount)
            {
                Console.Clear();
                Console.WriteLine("Введіть суму для зарахування (ціле або дробове число) або 'exit' для виходу:");

                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Операція скасована.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                if (decimal.TryParse(input, out amount) && amount > 0)
                {
                    validAmount = true;
                }
                else
                {
                    Console.WriteLine("Неправильне введення! Сума має бути числом більше нуля.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();

                }
            }

            account.Balance += amount;
            UpdateBalance(account);
            OnDeposit?.Invoke($"Успішно зараховано {amount} грн");
            LogTransaction(account.CardNumber, amount, "Deposit");
        }

static bool VerifyPin(string cardNumber, string pin)
{
    string hashedPin = PasswordHasher.HashPassword(pin);

    using (SqlConnection connection = new SqlConnection(connectionString))
    {
        string query = @"SELECT COUNT(*) 
                         FROM Accounts 
                         WHERE CardNumber = @CardNumber AND PIN = @PIN";
        SqlCommand command = new SqlCommand(query, connection);
        command.Parameters.AddWithValue("@CardNumber", cardNumber);
        command.Parameters.AddWithValue("@PIN", hashedPin);

        connection.Open();
        int count = (int)command.ExecuteScalar();

        return count > 0;
    }
}
        static void Transfer(Account account)
        {
            string receiverCardNumber = "";
            decimal amount = 0;
            bool validAmount = false;
            bool validReceiver = false;

            while (!validReceiver)
            {
                Console.Clear();
                Console.WriteLine("Введіть номер картки отримувача (4-значне число) або 'exit' для виходу:");
                receiverCardNumber = Console.ReadLine();

                if (receiverCardNumber.ToLower() == "exit")
                {
                    Console.WriteLine("Операція скасована.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                if (!IsValidCardNumber(receiverCardNumber))
                {
                    Console.WriteLine("Неправильне введення! Номер картки має бути 4-значним числом.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                }
                else
                {
                    validReceiver = true;
                }
            }

            while (!validAmount)
            {
                Console.Clear();
                Console.WriteLine("Введіть суму для перерахування (ціле або дробове число) або 'exit' для виходу:");
                string input = Console.ReadLine();

                if (input.ToLower() == "exit")
                {
                    Console.WriteLine("Операція скасована.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                    Console.Clear();
                    return;
                }

                if (decimal.TryParse(input, out amount) && amount > 0)
                {
                    validAmount = true;
                }
                else
                {
                    Console.WriteLine("Неправильне введення! Сума має бути числом більше нуля.");
                    Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                    Console.ReadKey();
                }
            }

            string receiverBank = GetBank(receiverCardNumber);

            decimal commission = 0;
            if (receiverBank != account.BankName)
            {
                commission = amount * 0.03m;
                Console.WriteLine($"Комісія за переказ на інший банк становить {commission} грн.");
            }

            decimal totalAmount = amount + commission;

            if (account.Balance >= totalAmount)
            {
                string pin = "";
                bool validPin = false;

                while (!validPin)
                {
                    Console.WriteLine("Підтвердіть свій пін-код (4-значне число):");
                    pin = Console.ReadLine();

                    if (!IsValidPIN(pin))
                    {
                        Console.WriteLine("Неправильне введення! PIN має бути 4-значним числом.");
                        Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                        Console.ReadKey();
                    }
                    else if (!VerifyPin(account.CardNumber, pin))
                    {
                        Console.WriteLine("Невірний PIN-код.");
                        Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                        Console.ReadKey();
                    }
                    else
                    {
                        validPin = true;
                    }
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    string query = "UPDATE Accounts SET Balance = Balance + @Amount WHERE CardNumber = @ReceiverCardNumber";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Amount", amount);
                    command.Parameters.AddWithValue("@ReceiverCardNumber", receiverCardNumber);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected > 0)
                    {
                        account.Balance -= totalAmount;
                        UpdateBalance(account);
                        OnTransfer?.Invoke($"Успішно перераховано {amount} грн на картку {receiverCardNumber}." +
                                           (commission > 0 ? $" Комісія: {commission} грн." : ""));
                        LogTransaction(account.CardNumber, amount, "Transfer", receiverCardNumber);
                    }
                    else
                    {
                        Console.WriteLine("Номер картки отримувача не знайдено.");
                        Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                        Console.ReadKey();
                    }
                }
            }
            else
            {
                Console.WriteLine("Недостатньо коштів.");
                Console.WriteLine("Натисніть будь-яку клавішу, щоб продовжити...");
                Console.ReadKey();
            }
        }

        static string GetBank(string cardNumber)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT b.BankName FROM Accounts a JOIN Banks b ON a.BankID = b.BankID WHERE a.CardNumber = @CardNumber";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CardNumber", cardNumber);

                connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    return reader["BankName"].ToString();
                }

                return null;
            }
        }


        static void UpdateBalance(Account account)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "UPDATE Accounts SET Balance = @Balance WHERE CardNumber = @CardNumber";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Balance", account.Balance);
                command.Parameters.AddWithValue("@CardNumber", account.CardNumber);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        static void LogTransaction(string cardNumber, decimal amount, string transactionType, string receiverCardNumber = null)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "INSERT INTO Transactions (AccountID, TransactionDate, Amount, TransactionType, ReceiverCardNumber) " +
                               "VALUES ((SELECT AccountID FROM Accounts WHERE CardNumber = @CardNumber), GETDATE(), @Amount, @TransactionType, @ReceiverCardNumber)";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@CardNumber", cardNumber);
                command.Parameters.AddWithValue("@Amount", amount);
                command.Parameters.AddWithValue("@TransactionType", transactionType);
                command.Parameters.AddWithValue("@ReceiverCardNumber", (object)receiverCardNumber ?? DBNull.Value);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        static void MessageHandler(string message)
        {
            Console.WriteLine(message);
        }
    }
}
