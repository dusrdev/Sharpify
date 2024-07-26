using System.Buffers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using MemoryPack;

using Sharpify.Collections;

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
    public bool TryGetValue(string key, out byte[] value) => TryGetValue(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    public bool TryGetValue(string key, string encryptionKey, out byte[] value) {
        try {
            _lock.EnterReadLock();
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                value = default!;
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                value = val!.ToArray();
                return true;
            }
            // Encrypted -> Decrypt
            value = Helper.Instance.Decrypt(val, encryptionKey);
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
    public RentedBufferWriter<byte> TryReadToRentedBuffer(string key, int reservedCapacity = 0)
        => TryReadToRentedBuffer(key, "", reservedCapacity);

    /// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
    public RentedBufferWriter<byte> TryReadToRentedBuffer(string key, string encryptionKey = "", int reservedCapacity = 0) {
        try {
            _lock.EnterReadLock();
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                return new RentedBufferWriter<byte>(0);
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                var buffer = new RentedBufferWriter<byte>(val!.Length + reservedCapacity);
                buffer.WriteAndAdvance(val);
                return buffer;
            } else {
                var buffer = new RentedBufferWriter<byte>(val!.Length + AesProvider.ReservedBufferSize + reservedCapacity);
                int numWritten = Helper.Instance.Decrypt(val, buffer.Buffer, encryptionKey);
                buffer.Advance(numWritten);
                return buffer;
            }
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
    public bool TryGetValue<T>(string key, out T value) where T : IMemoryPackable<T> => TryGetValue(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, out T value) where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                value = default!;
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                value = MemoryPackSerializer.Deserialize<T>(val.AsSpan(), _serializer.SerializerOptions)!;
                return true;
            }
            // Encrypted -> Decrypt
            var buffer = ArrayPool<byte>.Shared.Rent(val!.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            value = bytes.Length is 0 ? default! : MemoryPackSerializer.Deserialize<T>(bytes, _serializer.SerializerOptions)!;
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
    public bool TryGetValues<T>(string key, out T[] value) where T : IMemoryPackable<T> => TryGetValues(key, "", out value);

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="values">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, string encryptionKey, out T[] values) where T : IMemoryPackable<T> {
        try {
            _lock.EnterReadLock();
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                values = default!;
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                values = MemoryPackSerializer.Deserialize<T[]>(val.AsSpan(), _serializer.SerializerOptions)!;
                return true;
            }
            // Encrypted -> Decrypt
            var buffer = ArrayPool<byte>.Shared.Rent(val!.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            values = bytes.Length is 0 ? default! : MemoryPackSerializer.Deserialize<T[]>(bytes, _serializer.SerializerOptions)!;
            buffer.ReturnBufferToSharedArrayPool();
            return true;
        } finally {
            _lock.ExitReadLock();
        }
    }

    /// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
    public RentedBufferWriter<T> TryReadToRentedBuffer<T>(string key, int reservedCapacity = 0) where T : IMemoryPackable<T> => TryReadToRentedBuffer<T>(key, "", reservedCapacity);

    /// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
    public RentedBufferWriter<T> TryReadToRentedBuffer<T>(string key, string encryptionKey = "", int reservedCapacity = 0) where T : IMemoryPackable<T> {
        if (!TryGetValues<T>(key, encryptionKey, out var values)) {
            return new RentedBufferWriter<T>(0);
        }
        var buffer = new RentedBufferWriter<T>(values.Length + reservedCapacity);
        buffer.WriteAndAdvance(values);
        return buffer;
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, out string value) => TryGetString(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, string encryptionKey, out string value) {
        try {
            _lock.EnterReadLock();
            // Get val reference
            ref var val = ref _data.GetValueRefOrNullRef(key);
            if (Unsafe.IsNullRef(ref val)) { // Not found
                value = "";
                return false;
            }
            if (encryptionKey.Length is 0) { // Not encrypted
                value = MemoryPackSerializer.Deserialize<string>(val.AsSpan(), _serializer.SerializerOptions)!;
                return true;
            }
            // Encrypted -> Decrypt
            var buffer = ArrayPool<byte>.Shared.Rent(val!.Length + AesProvider.ReservedBufferSize);
            int length = Helper.Instance.Decrypt(val.AsSpan(), buffer, encryptionKey);
            var bytes = new ReadOnlySpan<byte>(buffer, 0, length);
            value = bytes.Length is 0 ? "" : MemoryPackSerializer.Deserialize<string>(bytes, _serializer.SerializerOptions)!;
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
    /// <param name="jsonTypeInfo"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, JsonTypeInfo<T> jsonTypeInfo, out T value) => TryGetValue(key, "", jsonTypeInfo, out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, JsonTypeInfo<T> jsonTypeInfo, out T value) {
        if (!TryGetString(key, encryptionKey, out string asString)) {
            value = default!;
            return false;
        }
        value = JsonSerializer.Deserialize(asString, jsonTypeInfo)!;
        return true;
    }
}