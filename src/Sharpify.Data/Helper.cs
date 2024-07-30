using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Sharpify.Data;

internal sealed class Helper : IDisposable {
    internal static readonly Helper Instance = new();

    private bool _disposed;

    private readonly ConcurrentDictionary<string, AesProvider> _cachedProviders = new(Environment.ProcessorCount, 1);

    /// <summary>
    /// Returns the encrypted version of the specified value using the specified key.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    internal byte[] Encrypt(ReadOnlySpan<byte> value, string key) {
        if (_cachedProviders.TryGetValue(key, out AesProvider? provider)) {
            return provider!.EncryptBytes(value);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.EncryptBytes(value);
    }

    /// <summary>
    /// Gets the encryptor for the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal ICryptoTransform GetEncryptor(string key) {
        if (_cachedProviders.TryGetValue(key, out AesProvider? provider)) {
            return provider!.CreateEncryptor();
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.CreateEncryptor();
    }

    /// <summary>
    /// Returns the decrypted version of the specified value using the specified key.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    internal byte[] Decrypt(ReadOnlySpan<byte> value, string key) {
        if (_cachedProviders.TryGetValue(key, out AesProvider? provider)) {
            return provider!.DecryptBytes(value);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.DecryptBytes(value, false);
    }

    /// <summary>
    /// Decrypts the specified value using the specified key and writes the result to the destination.
    /// </summary>
    /// <param name="value"></param>
    /// <param name="destination"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    internal int Decrypt(ReadOnlySpan<byte> value, Span<byte> destination, string key) {
        if (_cachedProviders.TryGetValue(key, out AesProvider? provider)) {
            return provider!.DecryptBytes(value, destination);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.DecryptBytes(value, destination, false);
    }

    /// <summary>
    /// Gets the decryptor for the specified key.
    /// </summary>
    /// <param name="key"></param>
    /// <returns></returns>
    internal ICryptoTransform GetDecryptor(string key) {
        if (_cachedProviders.TryGetValue(key, out AesProvider? provider)) {
            return provider!.CreateDecryptor();
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.CreateDecryptor();
    }

    public void Dispose() {
        if (_disposed) {
            return;
        }
        foreach (var provider in _cachedProviders.Values) {
            provider.Dispose();
        }
        _disposed = true;
    }
}