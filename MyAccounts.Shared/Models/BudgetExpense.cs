using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Shared.Models
{
    public class BudgetExpense
    {
        [Key]
        [DataMember]
        public long? Id { get; set; }
        [DataMember]
        public string Category { get; set; }
        [DataMember]
        public decimal APR { get; set; }
        [DataMember]
        public DateTime StartDate { get; set; }
        [DataMember]
        public DateTime DueDate { get; set; }
        [DataMember]
        public decimal CreditLimit { get; set; }
        [DataMember]
        public decimal MinPay { get; set; }
        [DataMember]
        public decimal Interest { get; set; }
        [DataMember]
        public decimal Balance { get; set; }
        [DataMember]
        public decimal Pay { get; set; }
        [DataMember]
        public string? Description { get; set; }
    }
}
