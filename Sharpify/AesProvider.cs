using System.Buffers;
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
    /// The reserved buffer size for the artifacts of encryption
    /// </summary>
    public const int ReservedBufferSize = 32;

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

    /// <summary>
    /// Finalizer
    /// </summary>
    ~AesProvider() {
        Dispose();
    }

    // Creates a usable fixed length key from the string password
    private static byte[] CreateKey(ReadOnlySpan<char> strKey) {
        var buffer = ArrayPool<byte>.Shared.Rent(strKey.Length * 2);
        int bytesWritten = Encoding.UTF8.GetBytes(strKey, buffer);
        ReadOnlySpan<byte> bytesSpan = buffer.AsSpan(0, bytesWritten);
        try {
            return SHA256.HashData(bytesSpan);
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }


    /// <summary>
    /// Hashes password
    /// </summary>
    /// <param name="password">password</param>
    /// <param name="iterations">hashing iteration - must be symmetric</param>
    /// <returns>hashed string</returns>
    public static string GeneratePassword(string password, int iterations = 991) {
        //generate a random salt for hashing
        //hash password given salt and iterations (default to 1000)
        //iterations provide difficulty when cracking
        using var pbkdf2 = new Rfc2898DeriveBytes(password, SaltSize, iterations, HashAlgorithmName.SHA512);
        var hash = pbkdf2.GetBytes(SaltSize);
        var salt = pbkdf2.Salt;

        // create format for hash text
        // salt|iterations|hash
        ReadOnlySpan<char> saltString = Convert.ToBase64String(salt);
        ReadOnlySpan<char> hashString = Convert.ToBase64String(hash);

        // length = salt + iteration count (3 digits max) + hash + 2 delimiters
        int length = saltString.Length + 3 + hashString.Length + 2;
        var buffer = StringBuffer.Create(stackalloc char[length]);
        buffer.Append(saltString);
        buffer.Append('|');
        buffer.Append(iterations);
        buffer.Append('|');
        buffer.Append(hashString);
        return buffer.Allocate(true);
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
    public string Encrypt(ReadOnlySpan<char> unencrypted) {
        var bytesBuffer = ArrayPool<byte>.Shared.Rent(unencrypted.Length * 2);
        int bytesWritten = Encoding.UTF8.GetBytes(unencrypted, bytesBuffer);
        ReadOnlySpan<byte> bytesSpan = bytesBuffer.AsSpan(0, bytesWritten);
        var encryptedBuffer = ArrayPool<byte>.Shared.Rent(bytesSpan.Length + ReservedBufferSize);
        int encryptedWritten = EncryptBytes(bytesSpan, encryptedBuffer);
        ReadOnlySpan<byte> encrypted = encryptedBuffer.AsSpan(0, encryptedWritten);
        try {
            return Convert.ToBase64String(encrypted);
        } finally {
            ArrayPool<byte>.Shared.Return(bytesBuffer);
            ArrayPool<byte>.Shared.Return(encryptedBuffer);
        }
    }

    /// <summary>
    /// Encrypts the text using the key
    /// </summary>
    /// <remarks>Returns an empty string if it fails</remarks>
    public string Decrypt(string encrypted) {
        var buffer = Convert.FromBase64String(encrypted);
        var decryptedBuffer = ArrayPool<byte>.Shared.Rent(buffer.Length);
        int decryptedWritten = DecryptBytes(buffer, decryptedBuffer);
        ReadOnlySpan<byte> decrypted = decryptedBuffer.AsSpan(0, decryptedWritten);
        try {
            return decrypted.Length is 0
                ? string.Empty
                : Encoding.UTF8.GetString(decrypted);
        } finally {
            ArrayPool<byte>.Shared.Return(decryptedBuffer);
        }
    }

    /// <summary>
    /// Encrypts the bytes using the key
    /// </summary>
    public byte[] EncryptBytes(ReadOnlySpan<byte> unencrypted) => _aes.EncryptCbc(unencrypted, _aes.IV);

    /// <summary>
    /// Encrypts the specified byte array using AES encryption in CBC mode.
    /// </summary>
    /// <param name="unencrypted">The byte array to be encrypted.</param>
    /// <param name="destination">The destination byte array to store the encrypted data.</param>
    /// <returns>The number of bytes written to the destination array.</returns>
    /// <remarks>The <paramref name="destination"/> length should be at least the same as <paramref name="unencrypted"/> + <see cref="ReservedBufferSize"/></remarks>
    public int EncryptBytes(ReadOnlySpan<byte> unencrypted, Span<byte> destination) => _aes.EncryptCbc(unencrypted, _aes.IV, destination);

    /// <summary>
    /// Decrypts the bytes using the key
    /// </summary>
    /// <remarks>Return an empty array if it failed</remarks>
    public byte[] DecryptBytes(ReadOnlySpan<byte> encrypted) {
        try {
            return _aes.DecryptCbc(encrypted, _aes.IV);
        } catch (CryptographicException) {
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Decrypts the specified encrypted bytes using AES in CBC mode.
    /// </summary>
    /// <param name="encrypted">The encrypted bytes to decrypt.</param>
    /// <param name="destination">The destination span to store the decrypted bytes.</param>
    /// <returns>The number of decrypted bytes.</returns>
    /// <remarks>The <paramref name="destination"/> length should be at least the same as <paramref name="encrypted"/></remarks>
    public int DecryptBytes(ReadOnlySpan<byte> encrypted, Span<byte> destination) {
        try {
            return _aes.DecryptCbc(encrypted, _aes.IV, destination);
        } catch (CryptographicException) {
            return 0;
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
        var buffer = ArrayPool<byte>.Shared.Rent(url.Length * 2);
        int bytesWritten = Encoding.UTF8.GetBytes(url, buffer);
        ReadOnlySpan<byte> bytesSpan = buffer.AsSpan(0, bytesWritten);
        var encryptedBuffer = ArrayPool<byte>.Shared.Rent(bytesSpan.Length + ReservedBufferSize);
        int encryptedWritten = EncryptBytes(bytesSpan, encryptedBuffer);
        ReadOnlySpan<byte> encrypted = encryptedBuffer.AsSpan(0, encryptedWritten);
        try {
            return Base64UrlEncode(encrypted);
        } finally {
            ArrayPool<byte>.Shared.Return(buffer);
            ArrayPool<byte>.Shared.Return(encryptedBuffer);
        }
    }

    /// <summary>
    /// Decrypts the url using the key
    /// </summary>
    /// <param name="encryptedUrl">encrypted url with Base64Url encoding</param>
    /// <returns>Decrypted url</returns>
    /// <remarks>Returns an empty string if it fails</remarks>
    public string DecryptUrl(string encryptedUrl) {
        var base64 = Base64UrlDecode(encryptedUrl);
        var decryptedBuffer = ArrayPool<byte>.Shared.Rent(base64.Length);
        int decryptedWritten = DecryptBytes(base64, decryptedBuffer);
        ReadOnlySpan<byte> decrypted = decryptedBuffer.AsSpan(0, decryptedWritten);
        try {
            return decrypted.Length is 0
                ? string.Empty
                : Encoding.UTF8.GetString(decrypted);
        } finally {
            ArrayPool<byte>.Shared.Return(decryptedBuffer);
        }
    }

    // Helper method to convert Base64Url encoded string to byte array
    private static byte[] Base64UrlDecode(string base64Url) {
#if NET8_0_OR_GREATER
        Span<char> buffer = stackalloc char[base64Url.Length + 2];
        base64Url.AsSpan().CopyTo(buffer);
        buffer.Replace('-', '+');
        buffer.Replace('_', '/');
        int mod = base64Url.Length % 4;
        int length = base64Url.Length;
        if (mod is 2) {
            "==".CopyTo(buffer[length..]);
            length += 2;
        } else if (mod is 3) {
            buffer[length] = '=';
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
    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) {
        string base64 = Convert.ToBase64String(bytes);
#if NET8_0_OR_GREATER
        Span<char> buffer = stackalloc char[base64.Length];
        base64.AsSpan().CopyTo(buffer);
        MemoryExtensions.Replace(buffer, '+', '-');
        MemoryExtensions.Replace(buffer, '/', '_');
        if (buffer[^1] is '=') {
            buffer = buffer[..^1];
        }
        return new string(buffer);
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
        GC.SuppressFinalize(this);
    }
}