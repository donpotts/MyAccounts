using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace MyAccounts.Shared.Models
{
    public class BudgetIncome
    {
        [Key]
        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string Source { get; set; }
        [DataMember]
        public decimal Amount { get; set; }
        [DataMember]
        public DateTime DateReceived { get; set; }
    }
}
