using System.ComponentModel.DataAnnotations;
using PassKee.Business.Common.Mvc.Attribute.Validation;
using PassKee.Business.Common.Utils;
using Xunit;

namespace PassKee.Tests.Unit.Utils;

public class PasswordValidatorTest
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_NullOrWhitespace_ShouldReturnInvalid(string? password)
    {
        var result = PasswordValidator.Validate(password);
        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("Ab1!")] // 4 chars
    [InlineData("Abcdefg1!")] // 9 chars (default min is 10)
    public void Validate_TooShort_ShouldReturnInvalid(string password)
    {
        var result = PasswordValidator.Validate(password);
        Assert.False(result.IsValid);
        Assert.Contains("at least 10", result.ErrorMessage);
    }

    [Fact]
    public void Validate_CustomMinLength_ShouldRespectParameter()
    {
        var result = PasswordValidator.Validate("Abc12345", minLength: 8);
        Assert.True(result.IsValid);

        var tooShort = PasswordValidator.Validate("Abc1234", minLength: 8);
        Assert.False(tooShort.IsValid);
    }

    [Fact]
    public void Validate_MissingDigit_ShouldReturnInvalid()
    {
        var result = PasswordValidator.Validate("NoDigitsHereTest");
        Assert.False(result.IsValid);
        Assert.Contains("digit", result.ErrorMessage);
    }

    [Fact]
    public void Validate_MissingUppercase_ShouldReturnInvalid()
    {
        var result = PasswordValidator.Validate("onlylowercase123");
        Assert.False(result.IsValid);
        Assert.Contains("uppercase", result.ErrorMessage);
    }

    [Fact]
    public void Validate_MissingLowercase_ShouldReturnInvalid()
    {
        var result = PasswordValidator.Validate("ONLYUPPERCASE123");
        Assert.False(result.IsValid);
        Assert.Contains("lowercase", result.ErrorMessage);
    }

    [Theory]
    [InlineData("ValidPass123")]
    [InlineData("Strong#P@ssw0rd!")]
    [InlineData("Correct-Horse-Battery-Staple-99")]
    [InlineData("aB3!12345678")]
    public void Validate_ValidPasswords_ShouldReturnValid(string password)
    {
        var result = PasswordValidator.Validate(password);
        Assert.True(result.IsValid);
        Assert.Null(result.ErrorMessage);

        Assert.True(PasswordValidator.IsValid(password, out var error));
        Assert.Null(error);
    }

    [Fact]
    public void StrongPasswordAttribute_ShouldValidateCorrectly()
    {
        var attr = new StrongPasswordAttribute(10);

        var validContext = new ValidationContext(new object());
        var validResult = attr.GetValidationResult("StrongPass123", validContext);
        Assert.Equal(ValidationResult.Success, validResult);

        var invalidResult = attr.GetValidationResult("weak", validContext);
        Assert.NotNull(invalidResult);
        Assert.NotEqual(ValidationResult.Success, invalidResult);
    }

    [Fact]
    public void GenerateStrongPassword_ShouldAlwaysPassValidator()
    {
        for (int i = 0; i < 50; i++)
        {
            var pwd = SecurityUtil.GenerateStrongPassword(16);
            Assert.Equal(16, pwd.Length);
            Assert.True(PasswordValidator.IsValid(pwd, out var error), $"Generated password '{pwd}' failed validation: {error}");
        }
    }

    [Fact]
    public void GenerateSecretKey_ShouldHaveExpectedFormat()
    {
        for (int i = 0; i < 20; i++)
        {
            var key = SecurityUtil.GenerateSecretKey();
            Assert.StartsWith("PK-", key);
            var parts = key.Split('-');
            Assert.Equal(7, parts.Length); // PK + 6 chunks of 4
            for (int p = 1; p < parts.Length; p++)
            {
                Assert.Equal(4, parts[p].Length);
            }
        }
    }

    [Fact]
    public void ParseSecretKey_WithStringKey_ShouldReturnUtf8Bytes()
    {
        var key = SecurityUtil.GenerateSecretKey();
        var bytes = CryptoUtils.ParseSecretKey(key);
        Assert.NotNull(bytes);
        Assert.Equal(System.Text.Encoding.UTF8.GetBytes(key), bytes);
    }

    [Fact]
    public void ParseSecretKey_WithLegacyBase64_ShouldDecodeCorrectly()
    {
        var rawBytes = CryptoUtils.GenerateRandomBytes(16);
        var base64Key = Convert.ToBase64String(rawBytes);

        var parsed = CryptoUtils.ParseSecretKey(base64Key);
        Assert.Equal(rawBytes, parsed);
    }

    [Fact]
    public void DeriveMasterKey_WithStringSecretKey_ShouldBeDeterministic()
    {
        var password = "MasterPassword#2026";
        var secretKey = SecurityUtil.GenerateSecretKey();
        var salt = CryptoUtils.GenerateRandomBytes(32);

        var key1 = CryptoUtils.DeriveMasterKey(password, secretKey, salt, 1, 1024, 1);
        var key2 = CryptoUtils.DeriveMasterKey(password, secretKey, salt, 1, 1024, 1);

        Assert.Equal(key1, key2);
    }

    [Fact]
    public void SessionEncrypt_And_Decrypt_WithStringSecretKey_ShouldRoundTrip()
    {
        var password = "MasterPassword#2026";
        var secretKey = SecurityUtil.GenerateSecretKey();
        var salt = CryptoUtils.GenerateRandomBytes(32);

        var encrypted = CryptoUtils.EncryptSecretKeyForSession(secretKey, password, salt);
        Assert.NotNull(encrypted);

        var decrypted = CryptoUtils.DecryptSecretKeyStringFromSession(encrypted, password, salt);
        Assert.Equal(secretKey, decrypted);
    }

    [Fact]
    public void PrepareClientRegistration_WithStringSecretKey_ShouldPreserveString()
    {
        var password = "MasterPassword#2026";
        var secretKey = SecurityUtil.GenerateSecretKey();

        var regData = CryptoUtils.PrepareClientRegistration(password, secretKey, iterations: 1, memorySize: 1024, parallelism: 1);
        Assert.Equal(secretKey, regData.SecretKeyString);
        Assert.NotNull(regData.MasterKey);
        Assert.NotNull(regData.AuthHash);
    }

    [Theory]
    [InlineData("PK-ABCD-EFGH-JKMN-PQRS-TVWX-YZ23")]
    [InlineData("pk-abcd-efgh-jkmn-pqrs-tvwx-yz23")]
    [InlineData("PKABCDEFGHJKMNPQRSTVWXYZ23")]
    [InlineData("  PK-ABCD EFGH JKMN PQRS TVWX YZ23\r\n")]
    public void LoginSecretKey_ShouldDeriveRegistrationAuthHash(string loginSecretKey)
    {
        const string password = "MasterPassword#2026";
        const string registrationSecretKey = "PK-ABCD-EFGH-JKMN-PQRS-TVWX-YZ23";
        var registration = CryptoUtils.PrepareClientRegistration(
            password, registrationSecretKey, iterations: 1, memorySize: 1024, parallelism: 1);

        var loginMasterKey = CryptoUtils.DeriveMasterKey(
            password, loginSecretKey, registration.AuthSalt,
            registration.KdfParams.Iterations, registration.KdfParams.MemorySize, registration.KdfParams.Parallelism);

        Assert.Equal(registration.AuthHash, CryptoUtils.ComputeAuthHash(loginMasterKey));
    }
}
