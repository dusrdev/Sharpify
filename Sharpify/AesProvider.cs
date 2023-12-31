using System.Security.Cryptography;
using System.Text;

using Sharpify.Collections;

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

        // create format for hash text
        // salt|iterations|hash
        var saltString = Convert.ToBase64String(salt);
        var hashString = Convert.ToBase64String(hash);

        // length = salt + iteration count (3 digits max) + hash + 2 delimiters
        int length = saltString.Length + 3 + hashString.Length + 2;
        var buffer = AllocatedStringBuffer.Create(stackalloc char[length]);
        buffer.Append(saltString);
        buffer.Append('|');
        buffer.Append(iterations);
        buffer.Append('|');
        buffer.Append(hashString);
        return buffer;
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
        ReadOnlySpan<byte> origSalt = Convert.FromBase64String(hashedPassword[parts[0]]);
        hpSpan[parts[1]].TryConvertToInt32(out var origIterations);
        ReadOnlySpan<char> origHash = hashedPassword[parts[2]];
#elif NET7_0_OR_GREATER
        var origHashedParts = hashedPassword.Split('|', 3, StringSplitOptions.RemoveEmptyEntries);
        ReadOnlySpan<byte> origSalt = Convert.FromBase64String(origHashedParts[0]);
        origHashedParts[1].AsSpan().TryConvertToInt32(out var origIterations);
        ReadOnlySpan<char> origHash = origHashedParts[2];
#endif

        //generate hash from test password and original salt and iterations

        var testHash = Rfc2898DeriveBytes.Pbkdf2(password, origSalt, origIterations, HashAlgorithmName.SHA512, SaltSize);

        ReadOnlySpan<char> testAsBase64 = Convert.ToBase64String(testHash);
        return testAsBase64.SequenceEqual(origHash);
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
#if NET8_0_OR_GREATER
        Span<char> buffer = stackalloc char[base64Url.Length + 2];
        var span = base64Url.AsSpan();
        span.Replace(buffer, '-', '+');
        MemoryExtensions.Replace(buffer, buffer, '_', '/');
        int mod = span.Length % 4;
        int length = span.Length;
        if (mod is 2) {
            "==".CopyTo(buffer[span.Length..]);
            length += 2;
        } else if (mod is 3) {
            buffer[span.Length] = '=';
            length += 1;
        }
        return Convert.FromBase64String(new string(buffer[0..length]));
#elif NET7_0
        var base64 = new StringBuilder(base64Url);
        base64.Replace('-', '+')
              .Replace('_', '/');
        switch (base64.Length % 4) {
            case 2: base64.Append("=="); break;
            case 3: base64.Append('='); break;
        }
        return Convert.FromBase64String(base64.ToString());
#endif
    }

    // Helper method to convert byte array to Base64Url encoded string
    private static string Base64UrlEncode(byte[] bytes) {
        var base64 = Convert.ToBase64String(bytes);
#if NET8_0_OR_GREATER
        // mutation is safe here because base64 is limited to the function scope anyway
        var mutableBuffer = Utils.Unsafe.AsMutableSpan<char>(base64);
        mutableBuffer.Replace('+', '-');
        mutableBuffer.Replace('/', '_');

        if (mutableBuffer[^1] is '=') {
            mutableBuffer = mutableBuffer[..^1];
        }

        return new string(mutableBuffer);
#elif NET7_0
        var sb = new StringBuilder(base64);
        sb.Replace('+', '-')
          .Replace('/', '_');
        while (sb[^1] is '=') {
            sb.Remove(sb.Length - 1, 1);
        }
        return sb.ToString();
#endif
    }

    /// <summary>
    /// Disposes the AES object
    /// </summary>
    public void Dispose() {
        _aes?.Dispose();
    }
}