using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using MemoryPack;

namespace Sharpify.Data;

public sealed partial class Database : IDisposable {
    /// <summary>
    /// Checked whether the inner dictionary contains the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    public bool ContainsKey(string key) => _data.ContainsKey(key);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    public bool TryGetValue(string key, out byte[] value) => TryGetValue(key, "", false, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    public bool TryGetValue(string key, string encryptionKey, out byte[] value)  => TryGetValue(key, encryptionKey, false, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <param name="lockValue">lock this value as an atomic upsert is using it</param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    internal bool TryGetValue(string key, string encryptionKey, bool lockValue, out byte[] value) {
        try {
            _lock.EnterReadLock();
            AcquireAtomicLock(key, lockValue);
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                value = Array.Empty<byte>();
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                value = val.FastCopy();
                return true;
            }
            // Encrypted -> Decrypt
            value = Helper.Instance.Decrypt(val.AsSpan(), encryptionKey);
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, out T value) where T : IMemoryPackable<T> => TryGetValue(key, "", false, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, out T value) where T : IMemoryPackable<T> {
        return TryGetValue(key, encryptionKey, false, out value);
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="lockValue">lock this value as an atomic upsert is using it</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    internal bool TryGetValue<T>(string key, string encryptionKey, bool lockValue, out T value) where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            AcquireAtomicLock(key, lockValue);
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                value = default!;
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                value = MemoryPackSerializer.Deserialize<T>(val.AsSpan())!;
                return true;
            }
            // Encrypted -> Decrypt
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            value = bytes.Length is 0 ? default! : MemoryPackSerializer.Deserialize<T>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, out T[] value) where T : IMemoryPackable<T> => TryGetValues(key, "", false, out value);

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="values">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, string encryptionKey, out T[] values) where T : IMemoryPackable<T> => TryGetValues(key, encryptionKey, false, out values);

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="lockValue">lock this value as an atomic upsert is using it</param>
    /// <param name="values">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    internal bool TryGetValues<T>(string key, string encryptionKey, bool lockValue, out T[] values) where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            AcquireAtomicLock(key, lockValue);
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                values = default!;
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                values = MemoryPackSerializer.Deserialize<T[]>(val.AsSpan())!;
                return true;
            }
            // Encrypted -> Decrypt
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            values = bytes.Length is 0 ? default! : MemoryPackSerializer.Deserialize<T[]>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, out string value) => TryGetString(key, "", false, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, string encryptionKey, out string value) => TryGetString(key, encryptionKey, false, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="lockValue">lock this value as an atomic upsert is using it</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    internal bool TryGetString(string key, string encryptionKey, bool lockValue, out string value) {
        try {
            _lock.EnterReadLock();
            AcquireAtomicLock(key, lockValue);
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                value = "";
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                value = MemoryPackSerializer.Deserialize<string>(val.AsSpan())!;
                return true;
            }
            // Encrypted -> Decrypt
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            value = bytes.Length is 0 ? "" : MemoryPackSerializer.Deserialize<string>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="jsonSerializerContext"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, JsonSerializerContext jsonSerializerContext, out T value) => TryGetValue(key, "", jsonSerializerContext, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="jsonSerializerContext"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, JsonSerializerContext jsonSerializerContext, out T value) {
        if (!TryGetString(key, encryptionKey, out string asString)) {
            value = default!;
            return false;
        }
        value = (T)JsonSerializer.Deserialize(asString, typeof(T), jsonSerializerContext)!;
        return true;
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as a byte[].
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <remarks>
    /// <para>This pure method which returns the value as byte[] allows you to use more complex but also more efficient serializers
    /// </para>
    /// <para>If the value doesn't exist null is returned. You can use this to check if a value exists.</para>
    /// </remarks>
    [Obsolete("Use TryGetValue instead.")]
    public byte[] Get(string key, string encryptionKey = "") {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return Array.Empty<byte>();
            }
            if (encryptionKey.Length is 0) {
                return val.FastCopy();
            }
            return Helper.Instance.Decrypt(val.AsSpan(), encryptionKey);
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Retrieves an object of type T from the database using the specified key.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <returns>The retrieved object of type T, or null if the object does not exist.</returns>
    [Obsolete("Use TryGetValue instead.")]
    public T? Get<T>(string key, string encryptionKey = "") where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return default;
            }
            if (encryptionKey.Length is 0) {
                return MemoryPackSerializer.Deserialize<T>(val.AsSpan());
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            var result = bytes.Length is 0 ? default : MemoryPackSerializer.Deserialize<T>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return result;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Returns the value for the <paramref name="key"/> as string. or empty string if the value doesn't exist.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    [Obsolete("Use TryGetString instead.")]
    public string GetAsString(string key, string encryptionKey = "") {
        try {
            _lock.EnterReadLock();
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) {
                return "";
            }
            if (encryptionKey.Length is 0) {
                return MemoryPackSerializer.Deserialize<string>(val.AsSpan())!;
            }
            var buffer = ArrayPool<byte>.Shared.Rent(val.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            var result = bytes.Length is 0 ? "" : MemoryPackSerializer.Deserialize<string>(bytes)!;
            buffer.ReturnBufferToSharedArrayPool();
            return result;
        } finally {
            _lock.ExitReadLock();
        }
    }
}