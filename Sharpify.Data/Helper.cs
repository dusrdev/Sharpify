using System.Collections.Concurrent;
using System.Security.Cryptography;

namespace Sharpify.Data;

internal sealed class Helper {
    internal static readonly Helper Instance = new();

    private readonly ConcurrentDictionary<string, AesProvider> _cachedProviders = new(Environment.ProcessorCount, 1);

    internal byte[] Encrypt(ReadOnlySpan<byte> value, string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.EncryptBytes(value);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.EncryptBytes(value);
    }

    internal ICryptoTransform GetEncryptor(string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.CreateEncryptor();
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.CreateEncryptor();
    }

    internal byte[] Decrypt(ReadOnlySpan<byte> value, string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.DecryptBytes(value);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.DecryptBytes(value);
    }

    internal int Decrypt(ReadOnlySpan<byte> value, Span<byte> destination, string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.DecryptBytes(value, destination);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.DecryptBytes(value, destination);
    }

    internal ICryptoTransform GetDecryptor(string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.CreateDecryptor();
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.CreateDecryptor();
    }

    ~Helper() {
        foreach (var provider in _cachedProviders.Values) {
            provider.Dispose();
        }
    }
}