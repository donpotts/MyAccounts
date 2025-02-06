using MyAccounts.Shared.Models;
using System.ComponentModel.DataAnnotations;

namespace MyAccounts.Models
{
    public class BudgetMonthlyBalance
    {
        [Key]
        public int BalanceId { get; set; }
        public int AccountId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Interest { get; set; }
        public decimal Balance { get; set; }
        public decimal Payment { get; set; }

        public BudgetAccount BudgetAccount { get; set; } // Navigation property
    }
}
