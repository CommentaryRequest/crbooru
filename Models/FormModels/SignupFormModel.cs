namespace CRbooru.Models.FormModels;

public class SignupFormModel
{
    public required string Username { get; set; }
    public required string Password { get; set; }
    public required string PasswordConfirmation { get; set; }
}
