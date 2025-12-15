using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Models
{
    public class BankCreditCardTransactionModel
    {
        public DateOnly? DateStart { get; set; }
        public DateOnly? DateEnd { get; set; }
        public string Description { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public Decimal Amount { get; set; }
        public string Type { get; set; }
        public string? Category { get; set; }
    }
}
