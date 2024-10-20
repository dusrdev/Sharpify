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
    private volatile bool _disposed;

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

    // Creates a usable fixed length key from the string password
    private static byte[] CreateKey(ReadOnlySpan<char> strKey) {
        using var buffer = new RentedBufferWriter<byte>(strKey.Length * sizeof(char));
        _ = Encoding.UTF8.GetBytes(strKey, buffer);
        return SHA256.HashData(buffer.WrittenSpan);
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
        return StringBuffer.Create(stackalloc char[length])
                           .Append(saltString)
                           .Append('|')
                           .Append(iterations)
                           .Append('|')
                           .Append(hashString)
                           .Allocate();
    }

    /// <summary>
    /// Validates that a password fits the hashed password
    /// </summary>
    /// <param name="password">password</param>
    /// <param name="hashedPassword">hashed password</param>
    /// <returns>bool</returns>
    public static bool IsPasswordValid(string password, string hashedPassword) {
        //extract original values from delimited hash text
        ReadOnlySpan<char> hpSpan = hashedPassword;
        Span<Range> parts = stackalloc Range[3];
        hpSpan.Split(parts, '|', StringSplitOptions.RemoveEmptyEntries
            | StringSplitOptions.TrimEntries);
        ReadOnlySpan<byte> origSalt = Convert.FromBase64String(hashedPassword[parts[0]]);
        hpSpan[parts[1]].TryConvertToInt32(out var origIterations);
        ReadOnlySpan<char> origHash = hashedPassword[parts[2]];

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
        using var bytesBuffer = new RentedBufferWriter<byte>(unencrypted.Length * sizeof(char));
        _ = Encoding.UTF8.GetBytes(unencrypted, bytesBuffer); // IBufferWriter overload advances automatically
        var writtenSpan = bytesBuffer.WrittenSpan;
        using var encryptedBuffer = new RentedBufferWriter<byte>(writtenSpan.Length + ReservedBufferSize);
        int encryptedWritten = EncryptBytes(writtenSpan, encryptedBuffer.GetSpan());
        encryptedBuffer.Advance(encryptedWritten);
        return Convert.ToBase64String(encryptedBuffer.WrittenSpan);
    }

    /// <summary>
    /// Encrypts the text using the key
    /// </summary>
    /// <remarks>Returns an empty string if it fails</remarks>
    public string Decrypt(string encrypted) {
        var buffer = Convert.FromBase64String(encrypted);
        using var decryptedBuffer = new RentedBufferWriter<byte>(buffer.Length);
        int decryptedWritten = DecryptBytes(buffer, decryptedBuffer.GetSpan());
        decryptedBuffer.Advance(decryptedWritten);
        ReadOnlySpan<byte> decrypted = decryptedBuffer.WrittenSpan;
        return decrypted.Length is 0
            ? string.Empty
            : Encoding.UTF8.GetString(decrypted);
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
    /// <param name="encrypted">encrypted bytes</param>
    /// <param name="throwOnError">throw exception on error</param>
    /// <returns>decrypted bytes</returns>
    /// <remarks>If <paramref name="throwOnError"/> is set to false, an empty array will be return in case of an error.</remarks>
    public byte[] DecryptBytes(ReadOnlySpan<byte> encrypted, bool throwOnError = false) {
        try {
            return _aes.DecryptCbc(encrypted, _aes.IV);
        } catch (CryptographicException) {
            if (throwOnError) {
                throw;
            }
            return Array.Empty<byte>();
        }
    }

    /// <summary>
    /// Decrypts the specified encrypted bytes using AES in CBC mode.
    /// </summary>
    /// <param name="encrypted">The encrypted bytes to decrypt.</param>
    /// <param name="destination">The destination span to store the decrypted bytes.</param>
    /// <param name="throwOnError">throw exception on error</param>
    /// <returns>The number of decrypted bytes.</returns>
    /// <remarks>
    /// <para>
    /// The <paramref name="destination"/> length should be at least the same as <paramref name="encrypted"/>
    /// </para>
    /// <para>
    /// If <paramref name="throwOnError"/> is set to false, an empty array will be return in case of an error.
    /// </para>
    ///</remarks>
    public int DecryptBytes(ReadOnlySpan<byte> encrypted, Span<byte> destination, bool throwOnError = false) {
        try {
            return _aes.DecryptCbc(encrypted, _aes.IV, destination);
        } catch (CryptographicException) {
            if (throwOnError) {
                throw;
            }
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
        using var buffer = new RentedBufferWriter<byte>(url.Length * sizeof(char));
        _ = Encoding.UTF8.GetBytes(url, buffer); // IBufferWriter overload advances automatically
        ReadOnlySpan<byte> bytesSpan = buffer.WrittenSpan;
        using var encryptedBuffer = new RentedBufferWriter<byte>(bytesSpan.Length + ReservedBufferSize);
        int encryptedWritten = EncryptBytes(bytesSpan, encryptedBuffer.GetSpan());
        encryptedBuffer.Advance(encryptedWritten);
        return Base64UrlEncode(encryptedBuffer.WrittenSpan);
    }

    /// <summary>
    /// Decrypts the url using the key
    /// </summary>
    /// <param name="encryptedUrl">encrypted url with Base64Url encoding</param>
    /// <returns>Decrypted url</returns>
    /// <remarks>Returns an empty string if it fails</remarks>
    public string DecryptUrl(string encryptedUrl) {
        var base64 = Base64UrlDecode(encryptedUrl);
        using var decryptedBuffer = new RentedBufferWriter<byte>(base64.Length);
        int decryptedWritten = DecryptBytes(base64, decryptedBuffer.GetSpan());
        decryptedBuffer.Advance(decryptedWritten);
        ReadOnlySpan<byte> decrypted = decryptedBuffer.WrittenSpan;
        return decrypted.Length is 0
            ? string.Empty
            : Encoding.UTF8.GetString(decrypted);
    }

    // Helper method to convert Base64Url encoded string to byte array
    private static byte[] Base64UrlDecode(string base64Url) {
        var length = base64Url.Length + 2;
        using var memoryOwner = MemoryPool<char>.Shared.Rent(length);
        Span<char> buffer = memoryOwner.Memory.Span.Slice(0, length);
        base64Url.AsSpan().CopyTo(buffer);
        buffer.Replace('-', '+');
        buffer.Replace('_', '/');
        int mod = base64Url.Length % 4;
        int nLength = base64Url.Length;
        if (mod is 2) {
            "==".CopyTo(buffer.Slice(nLength));
            nLength += 2;
        } else if (mod is 3) {
            buffer[nLength] = '=';
            nLength += 1;
        }
        return Convert.FromBase64String(new string(buffer.Slice(0, nLength)));
    }

    // Helper method to convert byte array to Base64Url encoded string
    private static string Base64UrlEncode(ReadOnlySpan<byte> bytes) {
        string base64 = Convert.ToBase64String(bytes);
        var length = base64.Length;
        using var memoryOwner = MemoryPool<char>.Shared.Rent(length);
        Span<char> buffer = memoryOwner.Memory.Span.Slice(0, length);
        base64.AsSpan().CopyTo(buffer);
        MemoryExtensions.Replace(buffer, '+', '-');
        MemoryExtensions.Replace(buffer, '/', '_');
        int last = buffer.Length - 1;
        if (buffer[last] is '=') {
            buffer = buffer.Slice(0, last);
        }
        return new string(buffer);
    }

    /// <summary>
    /// Disposes the AES object
    /// </summary>
    public void Dispose() {
        if (_disposed) {
            return;
        }
        _aes?.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }
}