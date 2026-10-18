namespace CRbooru.Models;

public class SignupModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string PasswordConfirmation { get; set; }
}
