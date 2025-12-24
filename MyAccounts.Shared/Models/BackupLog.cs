using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class BackupLog
{
    [Key]
    [DataMember]
    public long Id { get; set; }

    [DataMember]
    public DateTime CreatedAt { get; set; }

    [DataMember]
    public string? FileName { get; set; }

    [DataMember]
    public long FileSizeBytes { get; set; }

    [DataMember]
    public string? BackupType { get; set; } // "Manual" or "Scheduled"

    [DataMember]
    public bool Success { get; set; }

    [DataMember]
    public string? ErrorMessage { get; set; }
}
