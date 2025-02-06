using System.ComponentModel.DataAnnotations;

namespace MyAccounts.Models
{
    public class BudgetMiscellanousExpense
    {
        [Key]
        public int ExpenseId { get; set; }
        public string ExpenseType { get; set; }
        public decimal Amount { get; set; }
        public DateTime ExpenseDate { get; set; }
    }
}
