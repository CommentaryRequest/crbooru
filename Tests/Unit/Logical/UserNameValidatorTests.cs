using System.Text;
using CRbooru.Logical;
using CRbooru.Models;

namespace CRbooru.Tests.Unit.Logical;

public class UserNameValidatorTests
{
    [Theory]
    [InlineData("CommentaryRequest")]
    [InlineData("Type-kun")]
    [InlineData("Knowledge_Seeker")]
    [InlineData("The.Duke")]
    [InlineData("Anonymous9000")]
    public void Validate_ValidName_ReturnNull(string input)
    {
        Assert.Null(UserNameValidator.Validate(input));
    }

    [Fact]
    public void Validate_NameTooShort_ReturnMessage()
    {
        Assert.NotNull(UserNameValidator.Validate("a"));
    }

    [Fact]
    public void Validate_NameTooLong_ReturnMessage()
    {
        var count = User.MAX_USERNAME_LENGTH + 1;
        string input = new StringBuilder(count)
            .Insert(0, "a", count)
            .ToString();
        Assert.NotNull(UserNameValidator.Validate(input));
    }

    [Theory]
    [InlineData("Commentary Request")]
    [InlineData("☆♪")]
    [InlineData("$^%name")]
    public void Validate_InvalidName_ReturnMessage(string input)
    {
        Assert.NotNull(UserNameValidator.Validate(input));
    }
}
