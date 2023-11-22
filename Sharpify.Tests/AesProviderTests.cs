using System.Text;

namespace Sharpify.Tests;

public class AesProviderTests {
    private const string Key = "SuperSecretKey123";
    private const string PlainText = "Hello, World!";

    [Fact]
    public void Encrypt_Decrypt_ReturnsOriginalText() {
        using var aesProvider = new AesProvider(Key);
        string encrypted = aesProvider.Encrypt(PlainText);
        string decrypted = aesProvider.Decrypt(encrypted);

        decrypted.Should().Be(PlainText);
    }

    [Fact]
    public void EncryptBytes_DecryptBytes_ReturnsOriginalBytes() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] encryptedBytes = aesProvider.EncryptBytes(plainBytes);
        byte[] decryptedBytes = aesProvider.DecryptBytes(encryptedBytes);

        decryptedBytes.Should().Equal(plainBytes);
    }

    [Fact]
    public void DecryptBytes_WhenInputIsNotEncrypted_ReturnsEmptyString() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] decryptedBytes = aesProvider.DecryptBytes(plainBytes);

        decryptedBytes.Should().Equal(Array.Empty<byte>());
    }

    [Fact]
    public void GeneratePassword_IsPasswordValid_ReturnsTrueForValidPassword() {
        const string password = "Password123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void GeneratePassword_IsPasswordValid_ReturnsFalseForInvalidPassword() {
        const string password = "Password123";
        const string wrongPassword = "WrongPassword123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(wrongPassword, hashedPassword);

        isValid.Should().BeFalse();
    }

    [Fact]
    public void EncryptUrl_DecryptUrl_ValidInput_ReturnsOriginalUrl() {
        var cset = new Bogus.DataSets.System();
        using var aesProvider = new AesProvider(Key);

        for (int i = 0; i < 100; i++) {
            string url = cset.FileName();
            string encryptedUrl = aesProvider.EncryptUrl(url);
            string decryptedUrl = aesProvider.DecryptUrl(encryptedUrl);
            decryptedUrl.Should().Be(url);
        }
    }

    [Fact]
    public void EncryptUrl_WhenInputIsPlainText_ShouldReturnEncryptedUrl() {
        // Arrange
        var plainUrl = "testfile.txt";
        using var aesProvider = new AesProvider(Key);

        // Act
        var encryptedUrl = aesProvider.EncryptUrl(plainUrl);

        // Assert
        encryptedUrl.Should().NotBeNullOrEmpty();
        encryptedUrl.Should().NotBe(plainUrl);
    }

    [Fact]
    public void DecryptUrl_WhenInputIsEncryptedUrl_ShouldReturnDecryptedUrl() {
        // Arrange
        var plainUrl = "testfile.txt";
        using var aesProvider = new AesProvider(Key);
        var encryptedUrl = aesProvider.EncryptUrl(plainUrl);

        // Act
        var decryptedUrl = aesProvider.DecryptUrl(encryptedUrl);

        // Assert
        decryptedUrl.Should().NotBeNullOrEmpty();
        decryptedUrl.Should().Be(plainUrl);
    }

    [Fact]
    public void DecryptUrl_WhenInputIsIncorrectEncryptedUrl_ShouldNotReturnOriginalUrl() {
        // Arrange
        var plainUrl = "testfile.txt";
        using var aesProvider = new AesProvider(Key);
        var incorrectEncryptedUrl = aesProvider.EncryptUrl("incorrect_encrypted_filename");

        // Act
        var decryptedUrl = aesProvider.DecryptUrl(incorrectEncryptedUrl);

        // Assert
        decryptedUrl.Should().NotBeNull();
        decryptedUrl.Should().NotBe(plainUrl);
    }

    [Fact]
    public void EncryptAndDecryptUrl_WhenInputIsUnicode_ShouldReturnOriginalUrl() {
        // Arrange
        var unicodeUrl = "тестовый_файл.txt";
        using var aesProvider = new AesProvider(Key);

        // Act
        var encryptedUrl = aesProvider.EncryptUrl(unicodeUrl);
        var decryptedUrl = aesProvider.DecryptUrl(encryptedUrl);

        // Assert
        decryptedUrl.Should().NotBeNullOrEmpty();
        decryptedUrl.Should().Be(unicodeUrl);
    }
}