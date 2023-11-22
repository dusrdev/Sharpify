using System.Runtime.CompilerServices;
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
    private static readonly byte[] Vector = { 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 23, 19, 17 };
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
        var origHashedParts = hashedPassword.Split('|', StringSplitOptions.RemoveEmptyEntries);
        var origSalt = Convert.FromBase64String(origHashedParts[0]);
        int origIterations = 0;
        origHashedParts[1].AsSpan().ConvertToInt32Unsigned(ref origIterations);
        var origHash = origHashedParts[2];

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
    /// <param name="encrypted"></param>
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
    /// <param name="unencrypted"></param>
    public byte[] EncryptBytes(byte[] unencrypted) => _aes.EncryptCbc(unencrypted, _aes.IV);

    /// <summary>
    /// Decrypts the bytes using the key
    /// </summary>
    /// <param name="encrypted"></param>
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
        ref char current = ref Unsafe.As<string, char>(ref base64Url);
        ref char next = ref Unsafe.Add(ref current, 1);
        while (next is not '\0') {
            if (current is '-') {
                current = '+';
            } else if (current is '_') {
                current = '/';
            }
            current = ref next;
            next = ref Unsafe.Add(ref current, 1);
        }
        base64Url = base64Url.Concat((base64Url.Length % 4) switch {
            2 => "==",
            3 => "=",
            _ => ""
        });
        return Convert.FromBase64String(base64Url);
    }

    // Helper method to convert byte array to Base64Url encoded string
    private static string Base64UrlEncode(byte[] bytes) {
        var base64 = Convert.ToBase64String(bytes);
        ref char current = ref Unsafe.As<string, char>(ref base64);
        ref char next = ref Unsafe.Add(ref current, 1);
        while (next is not '\0') {
            if (current is '+') {
                current = '-';
            } else if (current is '/') {
                current = '_';
            }
            current = ref next;
            next = ref Unsafe.Add(ref current, 1);
        }
        return base64.TrimEnd('=');
    }

    /// <summary>
    /// Disposes the AES object
    /// </summary>
    public void Dispose() {
        _aes?.Dispose();
    }
}