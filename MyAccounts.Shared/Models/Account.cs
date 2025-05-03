using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class Account
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public string? Name { get; set; }

    [DataMember]
    public string? Note { get; set; }

    [DataMember]
    public bool? BudgetAccount { get; set; }

    [DataMember]
    public DateTime? Date { get; set; }

    [DataMember]
    public decimal? Balance { get; set; }

    [DataMember]
    public long? AccountTypeId { get; set; }

    [DataMember]
    public AccountType? AccountType { get; set; }

    [DataMember]
    public List<Category>? Category { get; set; }
}
