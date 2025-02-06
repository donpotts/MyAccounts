using System.ComponentModel.DataAnnotations;

namespace MyAccounts.Models
{
    public class BudgetIncome
    {
        [Key]
        public int IncomeId { get; set; }
        public string Source { get; set; }
        public decimal Amount { get; set; }
        public DateTime IncomeDate { get; set; }
    }
}
