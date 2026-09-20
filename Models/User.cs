using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

public enum UserRole : byte
{
    Member,
    Privileged,
    Builder,
    Moderator,
    Admin
}

[Index(nameof(Name), IsUnique = true)]
public class User
{
    public int Id { get; private set; }
    [MaxLength(25)]
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
}
