using System.Text.RegularExpressions;
using CRbooru.Models;

namespace CRbooru.Logical;

public static class UserNameValidator
{
    public static string? Validate(string name)
    {
        if (name.Length < User.MIN_USERNAME_LENGTH) {
            return $"Name must be at least {User.MIN_USERNAME_LENGTH} characters long";
        }

        if (name.Length > User.MAX_USERNAME_LENGTH) {
            return $"Name must not be longer than {User.MAX_USERNAME_LENGTH} characters";
        }

        if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_.-]+$")) {
            return "Name must only contain letters, numbers, underscores, periods and dashes.";
        }

        return null;
    }
}
