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
        generator.GetBytes(salt);

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
        var origHashedParts = hashedPassword.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
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
    public string Decrypt(string encrypted) {
        var buffer = Convert.FromBase64String(encrypted);
        var result = DecryptBytes(buffer);
        return Encoding.UTF8.GetString(result);
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
    public byte[] DecryptBytes(byte[] encrypted) => _aes.DecryptCbc(encrypted, _aes.IV);

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
    /// Disposes the AES object
    /// </summary>
    public void Dispose() {
        _aes?.Dispose();
    }
}