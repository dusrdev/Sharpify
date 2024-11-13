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

        decrypted.Should().Be(PlainText);
    }

    [Fact]
    public void AesProvider_Bytes() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] encryptedBytes = aesProvider.EncryptBytes(plainBytes);
        byte[] decryptedBytes = aesProvider.DecryptBytes(encryptedBytes);

        decryptedBytes.Should().Equal(plainBytes);
    }

    [Fact]
    public void AesProvider_Bytes_Span() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] encryptedBytes = aesProvider.EncryptBytes(plainBytes);
        Span<byte> decryptedSpan = stackalloc byte[plainBytes.Length];
        int written = aesProvider.DecryptBytes(encryptedBytes, decryptedSpan, true);

        decryptedSpan.Slice(0, written).SequenceEqual(plainBytes).Should().BeTrue();
    }

    [Fact]
    public void AesProvider_DecryptBytes_WhenInputIsNotEncrypted_ReturnsEmptyString() {
        byte[] plainBytes = Encoding.UTF8.GetBytes(PlainText);

        using var aesProvider = new AesProvider(Key);
        byte[] decryptedBytes = aesProvider.DecryptBytes(plainBytes);

        decryptedBytes.Should().Equal(Array.Empty<byte>());
    }

    [Fact]
    public void AesProvider_GeneratePassword_AndValidate() {
        const string password = "Password123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(password, hashedPassword);

        isValid.Should().BeTrue();
    }

    [Fact]
    public void AesProvider_Validate_Invalid() {
        const string password = "Password123";
        const string wrongPassword = "WrongPassword123";
        string hashedPassword = AesProvider.GeneratePassword(password);
        bool isValid = AesProvider.IsPasswordValid(wrongPassword, hashedPassword);

        isValid.Should().BeFalse();
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
            url.Should().Be(copy);
            decryptedUrl.Should().Be(url);
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
        encryptedUrl.Should().NotBeNullOrEmpty();
        encryptedUrl.Should().NotBe(plainUrl);
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
        decryptedUrl.Should().NotBeNullOrEmpty();
        decryptedUrl.Should().Be(plainUrl);
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
        decryptedUrl.Should().NotBeNull();
        decryptedUrl.Should().NotBe(plainUrl);
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
        decryptedUrl.Should().NotBeNullOrEmpty();
        decryptedUrl.Should().Be(unicodeUrl);
    }

    [Fact]
    public void AesProvider_CreateEncryptor() {
        using var aesProvider = new AesProvider(Key);
        var encryptor = aesProvider.CreateEncryptor();

        encryptor.Should().NotBeNull();

        var actual = encryptor.TransformFinalBlock(Encoding.UTF8.GetBytes(PlainText), 0, PlainText.Length);
        var expected = aesProvider.EncryptBytes(Encoding.UTF8.GetBytes(PlainText));

        actual.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public void AesProvider_CreateDecryptor() {
        using var aesProvider = new AesProvider(Key);
        var decryptor = aesProvider.CreateDecryptor();

        var source = aesProvider.EncryptBytes(Encoding.UTF8.GetBytes(PlainText));

        var actual = decryptor.TransformFinalBlock(source, 0, source.Length);
        var expected = aesProvider.DecryptBytes(source);

        actual.Should().BeEquivalentTo(expected);
    }
}