using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class BudgetAccount
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public string? AccountName { get; set; }

    [DataMember]
    public decimal? Apr { get; set; }

    [DataMember]
    public string? StartDate { get; set; }

    [DataMember]
    public string? DueDate { get; set; }

    [DataMember]
    public string? CreditLimit { get; set; }

    [DataMember]
    public decimal? MinPayment { get; set; }

}
