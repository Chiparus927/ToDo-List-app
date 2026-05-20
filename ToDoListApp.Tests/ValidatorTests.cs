using ToDoListApp.Utils;

namespace ToDoListApp.Tests;

public class ValidatorTests
{
    [Theory]
    [InlineData("ab", false)]
    [InlineData("abc", true)]
    [InlineData("  Ion Popescu  ", true)]
    [InlineData("", false)]
    [InlineData("   ", false)]
    public void IsFullNameValid_returns_expected(string fullName, bool expected)
    {
        Assert.Equal(expected, Validator.IsFullNameValid(fullName));
    }

    [Theory]
    [InlineData("user@example.com", true)]
    [InlineData("a@b.co", true)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    [InlineData("user@", false)]
    [InlineData("@domain.com", false)]
    public void IsEmailValid_returns_expected(string email, bool expected)
    {
        Assert.Equal(expected, Validator.IsEmailValid(email));
    }

    [Theory]
    [InlineData("Short1!", false)]
    [InlineData("noupper1!", false)]
    [InlineData("NoSpecial123", false)]
    [InlineData("ValidPass1!", true)]
    [InlineData("Another#9X", true)]
    [InlineData("", false)]
    public void IsPasswordValid_returns_expected(string password, bool expected)
    {
        Assert.Equal(expected, Validator.IsPasswordValid(password));
    }

    [Fact]
    public void IsNullOrWhiteSpace_true_when_any_empty()
    {
        Assert.True(Validator.IsNullOrWhiteSpace("a", ""));
        Assert.True(Validator.IsNullOrWhiteSpace(" ", "b"));
        Assert.True(Validator.IsNullOrWhiteSpace(null!, "x"));
    }

    [Fact]
    public void IsNullOrWhiteSpace_false_when_all_nonempty()
    {
        Assert.False(Validator.IsNullOrWhiteSpace("a", "b"));
    }
}
