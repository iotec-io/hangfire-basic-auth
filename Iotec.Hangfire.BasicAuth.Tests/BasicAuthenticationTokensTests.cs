namespace Iotec.Hangfire.BasicAuth.Tests;


public class BasicAuthenticationTokensTests
{
    [Fact]
    public void Are_Invalid_WhenWhenValid_ShouldReturnFalse()
    {
        var tokens = new[] {"user", "pass"};
        var authenticationTokens = new BasicAuthenticationTokens(tokens);

        var actual = authenticationTokens.Are_Invalid();
    
        Assert.False(actual);
    }

    [Theory]
    [InlineData("","")]
    [InlineData("", null)]
    [InlineData(null, "")]
    [InlineData("  ", " ")]
    [InlineData("user", " ")]
    [InlineData("", "pass")]
    [InlineData(null, null)]
    public void Are_Invalid_WhenWhenInvalid_ShouldReturnFalse(string token1, string token2)
    {
        var tokens = new[] { token1, token2 };
        var authenticationTokens = new BasicAuthenticationTokens(tokens);
 
        var actual = authenticationTokens.Are_Invalid();

        Assert.True(actual);
    }
    
    [Fact]
    public void Credentials_Match_Works()
    {
        var tokens = new[] {"user", "pass"};
        var authenticationTokens = new BasicAuthenticationTokens(tokens);
        var actual = authenticationTokens.Credentials_Match("user", "pass");
        Assert.True(actual);
    }
}