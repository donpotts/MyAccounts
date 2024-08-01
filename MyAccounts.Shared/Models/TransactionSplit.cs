using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class TransactionSplit
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public long? TransactionId { get; set; }

    [DataMember]
    public long? CategoryId { get; set; }

    [DataMember]
    public decimal? Amount { get; set; }

    [DataMember]
    public string? Notes { get; set; }

    [DataMember]
    public Transaction? Transaction { get; set; }

    [DataMember]
    public Category? Category { get; set; }
}
