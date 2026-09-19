using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace CRbooru.Models;

[Index(nameof(Name), IsUnique = true)]
public class User
{
    public int Id { get; private set; }
    [MaxLength(25)]
    public string Name { get; set; }

    public User(string name)
    {
        Name = name;
    }
}
