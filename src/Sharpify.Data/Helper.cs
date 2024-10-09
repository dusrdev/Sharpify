using System.Buffers.Binary;
using System.Collections.Concurrent;
using System.Security.Cryptography;

using MemoryPack;

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
    internal byte[] Encrypt(scoped ref readonly ReadOnlySpan<byte> value, string key) {
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
    internal byte[] Decrypt(scoped ref readonly ReadOnlySpan<byte> value, string key) {
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
    internal int Decrypt(scoped ref readonly ReadOnlySpan<byte> value, scoped Span<byte> destination, string key) {
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

    /// <summary>
    /// Returns the size of the serialized collection from the header
    /// </summary>
    /// <param name="data"></param>
    /// <remarks>
    /// Only use with MemoryPack
    /// </remarks>
    internal static int GetRequiredLength(ReadOnlySpan<byte> data) {
        const int lengthSize = 4; // 4 bytes for the length

        if (data.Length < lengthSize) {
            return 0;
        }

        var length = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(0, lengthSize));
        length = length == -1 ? 0 : length;

        return Math.Min(length, data.Length);
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