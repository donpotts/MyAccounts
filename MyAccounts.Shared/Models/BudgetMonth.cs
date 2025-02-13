using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Shared.Models
{
    public class BudgetMonth
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public MonthEnum Name { get; set; }
        [DataMember]
        public decimal TotalIncome { get; set; }
        [DataMember]
        public decimal TotalExpenses { get; set; }
        [DataMember]
        public decimal TotalRemaining { get; set; }
        [DataMember]
        public decimal Interest { get; set; }
        [DataMember]
        public decimal Payment { get; set; }
        [DataMember]
        public decimal Balance { get; set; }
    }

    public enum MonthEnum
    {
        January = 1,
        February,
        March,
        April,
        May,
        June,
        July,
        August,
        September,
        October,
        November,
        December
    }
}
