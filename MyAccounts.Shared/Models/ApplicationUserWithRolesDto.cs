using System.ComponentModel.DataAnnotations;

namespace MyAccounts.Shared.Models;

public class ApplicationUserWithRolesDto : ApplicationUserDto
{
    public List<string>? Roles { get; set; }
}
