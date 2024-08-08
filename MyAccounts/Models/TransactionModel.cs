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
        public DateTime Date { get; set; }
        public string Payee { get; set; }
        public string Category { get; set; }
        //public string Clr { get; set; }
        //public string CheckNumber { get; set; }
        public decimal Amount { get; set; }
        //public decimal Balance { get; set; }
        public string Account { get; set; }
    }

    public sealed class RecordMap : ClassMap<TransactionModel>
    {
        public RecordMap()
        {
           // Map(m => m.Id).Default(0); // Set default value to 0
            Map(m => m.Scheduled);
            Map(m => m.Split);
            Map(m => m.Date);
            Map(m => m.Payee);
            Map(m => m.Category);
            //Map(m => m.Clr).Name("Clr");
            //Map(m => m.CheckNumber).Name("Check #");
            Map(m => m.Amount);
            //Map(m => m.Balance);
            Map(m => m.Account);
        }
    }

}
