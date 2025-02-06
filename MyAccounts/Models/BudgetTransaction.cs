using MyAccounts.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MyAccounts.Models
{
    public class BudgetTransaction
    {
        [Key]
        public int TransactionId { get; set; }
        public int AccountId { get; set; }
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public decimal Amount { get; set; }

        public BudgetAccount BudgetAccount { get; set; } // Navigation property
    }
}
