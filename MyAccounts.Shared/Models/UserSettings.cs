using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class UserSettings
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    [Required]
    public string? UserId { get; set; }

    [DataMember]
    [Required]
    public string? SettingKey { get; set; }

    [DataMember]
    public string? SettingValue { get; set; }

    [DataMember]
    public DateTime? CreatedAt { get; set; }

    [DataMember]
    public DateTime? UpdatedAt { get; set; }
}
