using System.Text.Json;
using System.Text.Json.Serialization.Metadata;

using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data;

public sealed partial class Database {
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
    public bool TryGetValue(string key, out ReadOnlyMemory<byte> value) => TryGetValue(key, "", out value);

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key"></param>
    /// <param name="encryptionKey">individual encryption key for this specific value</param>
    /// <param name="value"></param>
    /// <returns>True if the value was found, false if not.</returns>
    public bool TryGetValue(string key, string encryptionKey, out ReadOnlyMemory<byte> value) {
        // Get val reference
        if (!_data.TryGetValue(key, out byte[]? val)) { // Not found
            value = ReadOnlyMemory<byte>.Empty;
            return false;
        }
        if (encryptionKey.Length is 0) { // Not encrypted
            value = val ?? ReadOnlyMemory<byte>.Empty;
            return true;
        }
        // Encrypted -> Decrypt
        ReadOnlySpan<byte> valSpan = val ?? ReadOnlySpan<byte>.Empty;
        value = Helper.Instance.Decrypt(valSpan, encryptionKey);
        return true;
    }

    /// <summary>
    /// Tries to get the values for the <paramref name="key"/> and write it to a <see cref="RentedBufferWriter{T}"/>
    /// </summary>
    /// <param name="key"></param>
    /// <param name="reservedCapacity">Reserved capacity after the values, useful to write additional data</param>
    /// <returns>
    /// A rented buffer writer containing the values if they were found, otherwise a disabled buffer writer (can be checked with <see cref="RentedBufferWriter{T}.IsDisabled"/>)
    /// </returns>
    public RentedBufferWriter<byte> TryReadToRentedBuffer(string key, int reservedCapacity = 0) {
        return TryReadToRentedBuffer(key, string.Empty, reservedCapacity);
    }


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
        // Get val reference
        if (!_data.TryGetValue(key, out byte[]? val)) { // Not found
            return new RentedBufferWriter<byte>(0);
        }
        if (encryptionKey.Length is 0) { // Not encrypted
            var buffer = new RentedBufferWriter<byte>(val!.Length + reservedCapacity);
            buffer.WriteAndAdvance(val);
            return buffer;
        } else {
            ReadOnlySpan<byte> valSpan = val;
            var buffer = new RentedBufferWriter<byte>(valSpan.Length + AesProvider.ReservedBufferSize + reservedCapacity);
            int numWritten = Helper.Instance.Decrypt(valSpan, buffer.Buffer, encryptionKey);
            buffer.Advance(numWritten);
            return buffer;
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, out T value) where T : IMemoryPackable<T> {
        return TryGetValue(key, string.Empty, out value);
    }


    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, out T value) where T : IMemoryPackable<T> {
        // Get val reference
        if (!_data.TryGetValue(key, out byte[]? val)) { // Not found
            value = default!;
            return false;
        }
        ReadOnlySpan<byte> valSpan = val;
        if (encryptionKey.Length is 0) { // Not encrypted
            value = MemoryPackSerializer.Deserialize<T>(valSpan, _serializer.SerializerOptions)!;
            return true;
        }
        // Encrypted -> Decrypt
        using var buffer = new RentedBufferWriter<byte>(valSpan.Length + AesProvider.ReservedBufferSize);
        int length = Helper.Instance.Decrypt(valSpan, buffer.GetSpan(), encryptionKey);
        buffer.Advance(length);
        if (length is 0) {
            value = default!;
            return false;
        } else {
            value = MemoryPackSerializer.Deserialize<T>(buffer.WrittenSpan, _serializer.SerializerOptions)!;
            return true;
        }
    }

    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, out T[] value) where T : IMemoryPackable<T> {
        return TryGetValues(key, string.Empty, out value);
    }


    /// <summary>
    /// Tries to get the value array stored in <paramref name="key"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to retrieve.</typeparam>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="values">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValues<T>(string key, string encryptionKey, out T[] values) where T : IMemoryPackable<T> {
        // Get val reference
        if (!_data.TryGetValue(key, out byte[]? val)) { // Not found
            values = Array.Empty<T>();
            return false;
        }
        ReadOnlySpan<byte> valSpan = val;
        if (encryptionKey.Length is 0) { // Not encrypted
            values = MemoryPackSerializer.Deserialize<T[]>(valSpan, _serializer.SerializerOptions)!;
            return true;
        }
        // Encrypted -> Decrypt
        using var buffer = new RentedBufferWriter<byte>(valSpan.Length + AesProvider.ReservedBufferSize);
        int length = Helper.Instance.Decrypt(valSpan, buffer.GetSpan(), encryptionKey);
        buffer.Advance(length);
        if (length is 0) {
            values = default!;
            return false;
        } else {
            values = MemoryPackSerializer.Deserialize<T[]>(buffer.WrittenSpan, _serializer.SerializerOptions)!;
            return true;
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
    public RentedBufferWriter<T> TryReadToRentedBuffer<T>(string key, int reservedCapacity = 0) where T : IMemoryPackable<T> {
        return TryReadToRentedBuffer<T>(key, string.Empty, reservedCapacity);
    }


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
        if (!TryGetValue(key, encryptionKey, out ReadOnlyMemory<byte> data)) {
            return new RentedBufferWriter<T>(0);
        }
        int dataLength = Helper.GetRequiredLength(data.Span);
        int bufferLength = dataLength + reservedCapacity;
        var buffer = new RentedBufferWriter<T>(bufferLength);
        Helper.ReadToRenterBufferWriter(ref buffer, data.Span, dataLength);
        return buffer;
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, out string value) {
        return TryGetString(key, string.Empty, out value);
    }


    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetString(string key, string encryptionKey, out string value) {
        // Get val reference
        if (!_data.TryGetValue(key, out byte[]? val)) { // Not found
            value = "";
            return false;
        }
        ReadOnlySpan<byte> valSpan = val;
        if (encryptionKey.Length is 0) { // Not encrypted
            value = MemoryPackSerializer.Deserialize<string>(valSpan, _serializer.SerializerOptions)!;
            return true;
        }
        // Encrypted -> Decrypt
        using var buffer = new RentedBufferWriter<byte>(valSpan.Length + AesProvider.ReservedBufferSize);
        int length = Helper.Instance.Decrypt(valSpan, buffer.GetSpan(), encryptionKey);
        buffer.Advance(length);
        if (length is 0) {
            value = "";
            return false;
        } else {
            value = MemoryPackSerializer.Deserialize<string>(buffer.WrittenSpan, _serializer.SerializerOptions)!;
            return true;
        }
    }

    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, JsonTypeInfo<T> jsonTypeInfo, out T value) {
        return TryGetValue(key, string.Empty, jsonTypeInfo, out value);
    }


    /// <summary>
    /// Tries to get the value for the <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key used to identify the object in the database.</param>
    /// <param name="encryptionKey">The encryption key used to decrypt the object if it is encrypted.</param>
    /// <param name="jsonTypeInfo"></param>
    /// <param name="value">The retrieved object of type T, or default if the object does not exist.</param>
    /// <returns>True if the value was found, otherwise false.</returns>
    public bool TryGetValue<T>(string key, string encryptionKey, JsonTypeInfo<T> jsonTypeInfo, out T value) {
        if (!TryGetValue(key, encryptionKey, out ReadOnlyMemory<byte> bytes)) {
            value = default!;
            return false;
        }
        value = JsonSerializer.Deserialize(bytes.Span, jsonTypeInfo)!;
        return true;
    }
}