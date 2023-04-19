using System.Text;

namespace Sharpify.Tests;

public class AesProviderTests {
    private const string Key = "SuperSecretKey123";
    private const string PlainText = "Hello, World!";

    [Fact]
    public void Encrypt_Decrypt_ReturnsOriginalText()
    {
        using var aesProvider = new AesProvider(Key);
        string encrypted = aesProvider.Encrypt(PlainText);
        string decrypted = aesProvider.Decrypt(encrypted);

        decrypted.Should().Be(PlainText);
    }

    [Fact]
    public void EncryptBytes_DecryptBytes_ReturnsOriginalBytes()
    {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] encryptedBytes = aesProvider.EncryptBytes(plainBytes);
        byte[] decryptedBytes = aesProvider.DecryptBytes(encryptedBytes);

        decryptedBytes.Should().Equal(plainBytes);
    }

    [Fact]
    public void GeneratePassword_IsPasswordValid_ReturnsTrueForValidPassword()
    {
        const string password = "Password123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void GeneratePassword_IsPasswordValid_ReturnsFalseForInvalidPassword()
    {
        const string password = "Password123";
        const string wrongPassword = "WrongPassword123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(wrongPassword, hashedPassword);

        isValid.Should().BeFalse();
    }
}