using CsvHelper.Configuration.Attributes;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace MyAccounts.Shared.Models;

[DataContract]
public class Category
{
    [Key]
    [DataMember]
    public long? Id { get; set; }

    [DataMember]
    public string? Name { get; set; }

    [DataMember]
    public long? ParentCategoryId { get; set; }

    [DataMember]
    public Category? ParentCategory { get; set; }

    [DataMember]
    [JsonIgnore]
    public List<Category>? SubCategories { get; set; }

    [DataMember]
    public List<Account>? Account { get; set; }

    [DataMember]
    public bool? BudgetCategory { get; set; }

    [NotMapped]
    [DataMember]
    public string? FullPath => ParentCategory != null
        ? $"{ParentCategory.Name} : {Name}"
        : Name;
}
