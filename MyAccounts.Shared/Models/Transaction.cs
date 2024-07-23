using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class Transaction
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public DateTime? Date { get; set; }

    [DataMember]
    public string? Payee { get; set; }

    [DataMember]
    public long? AccountId { get; set; }

    [DataMember]
    public long? CategoryId { get; set; }

    [DataMember]
    public bool Cleared { get; set; }

    [DataMember]
    public decimal? Amount { get; set; }

    [DataMember]
    public decimal? Balance { get; set; }

    [DataMember]
    public string? Description { get; set; }

    [DataMember]
    public Account? Account { get; set; }

    [DataMember]
    public List<Category>? Category { get; set; }
}
