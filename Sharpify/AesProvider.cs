using System.Security.Cryptography;
using System.Text;

namespace Sharpify;

/// <summary>
/// Provides easy to use AES encryption
/// </summary>
/// <remarks>
/// This implements <see cref="IDisposable"/>, make sure to properly dispose after use.
/// </remarks>
public sealed class AesProvider : IDisposable {
    private static readonly byte[] Vector = [181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 23, 19, 17];
    private readonly Aes _aes;

    private const int SaltSize = 24;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="strKey">Encryption key as string</param>
    public AesProvider(string strKey) {
        _aes = Aes.Create();
        _aes.KeySize = 256;
        _aes.BlockSize = 128;
        _aes.FeedbackSize = 8;
        _aes.Padding = PaddingMode.PKCS7;
        _aes.Key = CreateKey(strKey);
        _aes.IV = Vector;
        _aes.Mode = CipherMode.CBC;
    }

    // Creates a usable fixed length key from the string password
    private static byte[] CreateKey(string strKey) => SHA256.HashData(Encoding.UTF8.GetBytes(strKey));

    /// <summary>
    /// Hashes password
    /// </summary>
    /// <param name="password">password</param>
    /// <param name="iterations">hashing iteration - must be symmetric</param>
    /// <returns>hashed string</returns>
    public static string GeneratePassword(string password, int iterations = 991) {
        //generate a random salt for hashing
        var salt = new byte[SaltSize];
        using var generator = RandomNumberGenerator.Create();
        generator.GetBytes(salt.AsSpan());

        //hash password given salt and iterations (default to 1000)
        //iterations provide difficulty when cracking
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, HashAlgorithmName.SHA512);
        var hash = pbkdf2.GetBytes(SaltSize);

        //return delimited string with salt | #iterations | hash
        return string.Join('|', Convert.ToBase64String(salt), iterations, Convert.ToBase64String(hash));
    }

    /// <summary>
    /// Validates that a password fits the hashed password
    /// </summary>
    /// <param name="password">password</param>
    /// <param name="hashedPassword">hashed password</param>
    /// <returns>bool</returns>
    public static bool IsPasswordValid(string password, string hashedPassword) {
        //extract original values from delimited hash text
#if NET8_0_OR_GREATER
        ReadOnlySpan<char> hpSpan = hashedPassword;
        Span<Range> parts = stackalloc Range[3];
        hpSpan.Split(parts, '|', StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries);
        var origSalt = Convert.FromBase64String(hashedPassword[parts[0]]);
        int origIterations = 0;
        hpSpan[parts[1]].ConvertToInt32Unsigned(ref origIterations);
        var origHash = hashedPassword[parts[2]];
#elif NET7_0_OR_GREATER
        var origHashedParts = hashedPassword.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var origSalt = Convert.FromBase64String(origHashedParts[0]);
        int origIterations = 0;
        origHashedParts[1].AsSpan().ConvertToInt32Unsigned(ref origIterations);
        var origHash = origHashedParts[2];
#endif

        //generate hash from test password and original salt and iterations
        using var pbkdf2 = new Rfc2898DeriveBytes(password, origSalt, origIterations, HashAlgorithmName.SHA512);
        var testHash = pbkdf2.GetBytes(SaltSize);

        return Convert.ToBase64String(testHash) == origHash;
    }

    /// <summary>
    /// Encrypts the text using the key
    /// </summary>
    /// <param name="unencrypted">original text</param>
    /// <returns>Unicode string</returns>
    public string Encrypt(string unencrypted) {
        var buffer = Encoding.UTF8.GetBytes(unencrypted);
        var result = EncryptBytes(buffer);
        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// Encrypts the text using the key
    /// </summary>
    /// <remarks>Returns an empty string if it fails</remarks>
    public string Decrypt(string encrypted) {
        var buffer = Convert.FromBase64String(encrypted);
        var result = DecryptBytes(buffer);
        return result.Length is 0
            ? string.Empty
            : Encoding.UTF8.GetString(result);
    }

    /// <summary>
    /// Encrypts the bytes using the key
    /// </summary>
    public byte[] EncryptBytes(byte[] unencrypted) => _aes.EncryptCbc(unencrypted, _aes.IV);

    /// <summary>
    /// Decrypts the bytes using the key
    /// </summary>
    /// <remarks>Return an empty array if it failed</remarks>
    public byte[] DecryptBytes(byte[] encrypted) {
        try {
            return _aes.DecryptCbc(encrypted, _aes.IV);
        } catch (CryptographicException) {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Creates an encryptor
    /// </summary>
    /// <remarks>
    /// <see cref="ICryptoTransform"/> implements <see cref="IDisposable"/>, make sure to properly dispose it.
    /// </remarks>
    public ICryptoTransform CreateEncryptor() => _aes.CreateEncryptor();

    /// <summary>
    /// Creates a decryptor
    /// </summary>
    /// <remarks>
    /// <see cref="ICryptoTransform"/> implements <see cref="IDisposable"/>, make sure to properly dispose it.
    /// </remarks>
    public ICryptoTransform CreateDecryptor() => _aes.CreateDecryptor();

    /// <summary>
    /// Encrypts the url using the key
    /// </summary>
    /// <param name="url">original url</param>
    /// <returns>Encrypted url with Base64Url encoding</returns>
    public string EncryptUrl(string url) {
        var encryptedBytes = EncryptBytes(Encoding.UTF8.GetBytes(url));
        return Base64UrlEncode(encryptedBytes);
    }

    /// <summary>
    /// Decrypts the url using the key
    /// </summary>
    /// <param name="encryptedUrl">encrypted url with Base64Url encoding</param>
    /// <returns>Decrypted url</returns>
    /// <remarks>Returns an empty string if it fails</remarks>
    public string DecryptUrl(string encryptedUrl) {
        var decryptedBytes = DecryptBytes(Base64UrlDecode(encryptedUrl));
        return decryptedBytes.Length is 0
            ? string.Empty
            : Encoding.UTF8.GetString(decryptedBytes);
    }

    // Helper method to convert Base64Url encoded string to byte array
    private static byte[] Base64UrlDecode(string base64Url) {
        var base64 = new StringBuilder(base64Url);
        base64.Replace('-', '+')
              .Replace('_', '/');
        switch (base64.Length % 4) {
            case 2: base64.Append("=="); break;
            case 3: base64.Append('='); break;
        }
        return Convert.FromBase64String(base64.ToString());
    }

    // Helper method to convert byte array to Base64Url encoded string
    private static string Base64UrlEncode(byte[] bytes) {
        var base64 = Convert.ToBase64String(bytes);
        var sb = new StringBuilder(base64);
        sb.Replace('+', '-')
          .Replace('/', '_');
        while (sb[^1] is '=') {
            sb.Remove(sb.Length - 1, 1);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Disposes the AES object
    /// </summary>
    public void Dispose() {
        _aes?.Dispose();
    }
}