using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Models
{
    public class BankTransactionModel
    {
        public DateOnly? Date { get; set; }
        public string Time { get; set; }
        [Column(TypeName = "decimal(18, 2)")]
        public Decimal Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
