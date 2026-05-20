using ToDoListApp.Utils;

namespace ToDoListApp.Tests;

public class HelpersTests
{
    [Fact]
    public void HashPassword_is_deterministic()
    {
        const string password = "TestPass1!";
        var a = Helpers.HashPassword(password);
        var b = Helpers.HashPassword(password);
        Assert.Equal(a, b);
    }

    [Fact]
    public void HashPassword_produces_64_char_hex_uppercase()
    {
        var hash = Helpers.HashPassword("AnyValue1!");
        Assert.Equal(64, hash.Length);
        Assert.True(hash.All(c => char.IsAsciiHexDigit(c)));
        Assert.Equal(hash, hash.ToUpperInvariant());
    }

    [Fact]
    public void HashPassword_different_inputs_different_output()
    {
        var h1 = Helpers.HashPassword("Pass1!");
        var h2 = Helpers.HashPassword("Pass2!");
        Assert.NotEqual(h1, h2);
    }
}
