using CsvHelper.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Models
{
    public class TransactionModel
    {
        //public long Id { get; set; }
        public string Scheduled { get; set; }
        public string Split { get; set; }
        public DateOnly Date { get; set; }
        public string Payee { get; set; }
        public string Category { get; set; }
        //public string Clr { get; set; }
        //public string CheckNumber { get; set; }
        public decimal Amount { get; set; }
        //public decimal Balance { get; set; }
        public string Account { get; set; }
    }
}
