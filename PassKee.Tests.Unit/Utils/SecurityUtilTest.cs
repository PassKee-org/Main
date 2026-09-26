using PassKee.Business.Common.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Utils;

public class SecurityUtilTest
{
    [Fact]
    public void GenerateSalt_ShouldReturnExpectedLength()
    {
        var salt1 = SecurityUtil.GenerateSalt(16);
        Assert.Equal(16, salt1.Length);

        var salt2 = SecurityUtil.GenerateSalt(32);
        Assert.Equal(32, salt2.Length);
        Assert.NotEqual(salt1, salt2);
    }

    [Fact]
    public void GeneratePassword_ShouldReturnRequestedLength()
    {
        var password = SecurityUtil.GeneratePassword(20);
        Assert.Equal(20, password.Length);

        var defaultPassword = SecurityUtil.GeneratePassword();
        Assert.Equal(12, defaultPassword.Length);
    }

    [Fact]
    public void GeneratePasswordHash_ShouldGenerateConsistentPbkdf2Hash()
    {
        var salt = SecurityUtil.GenerateSalt(16);
        var hash1 = SecurityUtil.GeneratePasswordHash("secretPass", salt, 100);
        var hash2 = SecurityUtil.GeneratePasswordHash("secretPass", salt, 100);

        Assert.Equal(hash1, hash2);
        Assert.NotEmpty(hash1);
    }

    [Fact]
    public void GetTimeBasedToken_ShouldGenerateUniqueToken()
    {
        var token1 = SecurityUtil.GetTimeBasedToken(false);
        var token2 = SecurityUtil.GetTimeBasedToken(true);

        Assert.NotEmpty(token1);
        Assert.NotEmpty(token2);
        Assert.NotEqual(token1, token2);
    }
}

