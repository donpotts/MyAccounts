using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Models
{
    public class BankTransactionModel
    {
        public DateOnly? Date { get; set; }
        public string Time { get; set; }
        public Decimal Amount { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }
    }
}
