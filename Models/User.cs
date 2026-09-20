using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

public enum UserRole : byte
{
    Member = 1,
    Privileged = 2,
    Builder = 3,
    Moderator = 4,
    Admin = 5
}

[Index(nameof(Name), IsUnique = true)]
public class User
{
    public const int MAX_USERNAME_LENGTH = 25;
    public const int MIN_USERNAME_LENGTH = 2;

    public int Id { get; private set; }
    [MaxLength(MAX_USERNAME_LENGTH)]
    public string Name { get; set; }
    public string PasswordHash { get; set; }
    public UserRole Role { get; set; }
}
