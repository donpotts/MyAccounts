using System.ComponentModel.DataAnnotations;

namespace MyAccounts.Models
{
    public class BudgetAccount
    {
        [Key]
        public int AccountId { get; set; }
        public string AccountName { get; set; }
        public decimal? APR { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? DueDate { get; set; }
        public decimal? CreditLimit { get; set; }
        public decimal? MinPayment { get; set; }
    }
}
