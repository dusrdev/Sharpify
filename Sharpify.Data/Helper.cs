using System.Collections.Concurrent;

namespace Sharpify.Data;

internal sealed class Helper {
    internal static readonly Helper Instance = new();

    private readonly ConcurrentDictionary<string, AesProvider> _cachedProviders = new();

    internal byte[] Encrypt(ReadOnlySpan<byte> value, string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.EncryptBytes(value);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.EncryptBytes(value);
    }

    internal int Encrypt(ReadOnlySpan<byte> value, Span<byte> destination, string key) {
        if (_cachedProviders.TryGetValue(key, out var provider)) {
            return provider.EncryptBytes(value, destination);
        }
        var newProvider = new AesProvider(key);
        _cachedProviders.TryAdd(key, newProvider);
        return newProvider.EncryptBytes(value, destination);
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

    ~Helper() {
        foreach (var provider in _cachedProviders.Values) {
            provider.Dispose();
        }
    }
}