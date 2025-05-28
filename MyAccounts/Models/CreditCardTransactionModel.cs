using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Models
{
    public class CreditCardTransactionModel
    {
        public DateOnly? DateStart { get; set; }
        public DateOnly? DateEnd { get; set; }
        public string Description { get; set; }
        public Decimal Amount { get; set; }
        public string Type { get; set; }
        
    }
}
