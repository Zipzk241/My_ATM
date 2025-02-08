# ATM Console Program

## Overview
This is a simple console-based ATM simulation written in C#. The program allows users to authenticate using a card number and PIN, check their balance, withdraw and deposit money, transfer funds.

## Features
- **User Authentication**: Secure login using card number and PIN.
- **Balance Inquiry**: View current account balance.
- **Cash Withdrawal**: Withdraw money with validation.
- **Deposit Funds**: Add money to an account.
- **Money Transfer**: Transfer funds to another account.
- **Error Handling**: Ensures proper input validation and error messages.

## Technologies Used
- **C#** (Console Application)
- **SQL Server**

## Installation & Setup
### Prerequisites
- .NET SDK installed
- SQL Server configured with the required database schema

### Steps to Run
1. Clone the repository:
   ```sh
   git clone https://github.com/Zipzk241/My_ATM.git
   ```
2. Navigate to the project directory:
   ```sh
   cd atm
   ```
3. Restore dependencies:
   ```sh
   dotnet restore
   ```
4. Configure the database connection string in `appsettings.json`.
5. Run the application:
   ```sh
   dotnet run
   ```

## Database Schema
The application uses the following main tables:
- **Accounts** (`AccountID`, `CardNumber`, `PIN`, `OwnerName`, `Balance`, `IsLocked`, `BankID`)
- **Transactions** (`TransactionID`, `AccountID`, `TransactionDate`, `Amount`, `TransactionType`, `ReveiverCardNumber`)
- **Banks** (`BankID`, `BankName`, `Address`)