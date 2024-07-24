using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyAccounts.Shared.Models;

namespace MyAccounts.Services
{
    public class TransactionService
    {
        public void CalculateBalances(List<Transaction> transactions)
        {
            decimal? balance = 0;
            foreach (var transaction in transactions.OrderBy(t => t.Date).ThenBy(x => x.Id))
            {
                balance += transaction.Amount;
                transaction.Balance = balance;
            }
        }
    }
}
