using System.Text;

namespace Sharpify.Tests;

public class AesProviderTests {
    private const string Key = "SuperSecretKey123";
    private const string PlainText = "Hello, World!";

    [Fact]
    public void AesProvider_PlainText() {
        using var aesProvider = new AesProvider(Key);
        string encrypted = aesProvider.Encrypt(PlainText);
        string decrypted = aesProvider.Decrypt(encrypted);

        Assert.Equal(PlainText, decrypted);
    }

    [Fact]
    public void AesProvider_Bytes() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] encryptedBytes = aesProvider.EncryptBytes(plainBytes);
        byte[] decryptedBytes = aesProvider.DecryptBytes(encryptedBytes);

        Assert.Equal(plainBytes, decryptedBytes);
    }

    [Fact]
    public void AesProvider_Bytes_Span() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] encryptedBytes = aesProvider.EncryptBytes(plainBytes);
        Span<byte> decryptedSpan = stackalloc byte[plainBytes.Length];
        int written = aesProvider.DecryptBytes(encryptedBytes, decryptedSpan, true);

        Assert.Equal(plainBytes, decryptedSpan.Slice(0, written));
    }

    [Fact]
    public void AesProvider_DecryptBytes_WhenInputIsNotEncrypted_ReturnsEmptyString() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] decryptedBytes = aesProvider.DecryptBytes(plainBytes);

        Assert.Equal(Array.Empty<byte>(), decryptedBytes);
    }

    [Fact]
    public void AesProvider_GeneratePassword_AndValidate() {
        const string password = "Password123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(password, hashedPassword);

        Assert.True(isValid);
    }

    [Fact]
    public void AesProvider_Validate_Invalid() {
        const string password = "Password123";
        const string wrongPassword = "WrongPassword123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(wrongPassword, hashedPassword);

        Assert.False(isValid);
    }

    [Fact]
    public void AesProvider_URL() {
        var cset = new Bogus.DataSets.System();
        using var aesProvider = new AesProvider(Key);

        for (int i = 0; i < 100; i++) {
            string url = cset.FileName();
            string copy = new(url);
            string encryptedUrl = aesProvider.EncryptUrl(url);
            string decryptedUrl = aesProvider.DecryptUrl(encryptedUrl);
            Assert.Equal(copy, url);
            Assert.Equal(url, decryptedUrl);
        }
    }

    [Fact]
    public void AesProvider_EncryptUrl_OnPlainText() {
        // Arrange
        var plainUrl = "testfile.txt";
        using var aesProvider = new AesProvider(Key);

        // Act
        var encryptedUrl = aesProvider.EncryptUrl(plainUrl);

        // Assert
        Assert.NotNull(encryptedUrl);
        Assert.NotEmpty(encryptedUrl);
        Assert.NotEqual(plainUrl, encryptedUrl);
    }

    [Fact]
    public void AesProvider_DecryptUrl_OnEncryptedText() {
        // Arrange
        var plainUrl = "testfile.txt";
        using var aesProvider = new AesProvider(Key);
        var encryptedUrl = aesProvider.EncryptUrl(plainUrl);

        // Act
        var decryptedUrl = aesProvider.DecryptUrl(encryptedUrl);

        // Assert
        Assert.NotNull(decryptedUrl);
        Assert.NotEmpty(decryptedUrl);
        Assert.Equal(plainUrl, decryptedUrl);
    }

    [Fact]
    public void AesProvider_DecryptUrl_IncorrectEncryptedUrl() {
        // Arrange
        var plainUrl = "testfile.txt";
        using var aesProvider = new AesProvider(Key);
        var incorrectEncryptedUrl = aesProvider.EncryptUrl("incorrect_encrypted_filename");

        // Act
        var decryptedUrl = aesProvider.DecryptUrl(incorrectEncryptedUrl);

        // Assert
        Assert.NotNull(decryptedUrl);
        Assert.NotEqual(plainUrl, decryptedUrl);
    }

    [Fact]
    public void AesProvider_EncryptAndDecryptUrl_WhenInputIsUnicode() {
        // Arrange
        var unicodeUrl = "тестовый_файл.txt";
        using var aesProvider = new AesProvider(Key);

        // Act
        var encryptedUrl = aesProvider.EncryptUrl(unicodeUrl);
        var decryptedUrl = aesProvider.DecryptUrl(encryptedUrl);

        // Assert
        Assert.NotNull(decryptedUrl);
        Assert.NotEmpty(decryptedUrl);
        Assert.Equal(unicodeUrl, decryptedUrl);
    }

    [Fact]
    public void AesProvider_CreateEncryptor() {
        using var aesProvider = new AesProvider(Key);
        var encryptor = aesProvider.CreateEncryptor();

        Assert.NotNull(encryptor);

        var actual = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(PlainText), 0, PlainText.Length);
        var expected = aesProvider.EncryptBytes(Encoding.UTF8.GetBytes(PlainText));

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void AesProvider_CreateDecryptor() {
        using var aesProvider = new AesProvider(Key);
        var decryptor = aesProvider.CreateDecryptor();

        var source = aesProvider.EncryptBytes(Encoding.UTF8.GetBytes(PlainText));

        var actual = decryptor.TransformFinalBlock(source, 0, source.Length);
        var expected = aesProvider.DecryptBytes(source);

        Assert.Equal(expected, actual);
    }
}