using MemoryPack;

using Sharpify.Collections;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class Serializer : DatabaseSerializer {
    internal Serializer(string path) : base(path) {
    }

/// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize(int estimatedSize) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>();
        }
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        using var file = new FileStream(_path, FileMode.Open);
        var numRead = file.Read(buffer.Buffer, 0, estimatedSize);
        buffer.Advance(numRead);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            MemoryPackSerializer.Deserialize<Dictionary<string, ReadOnlyMemory<byte>>>(buffer.WrittenSpan)
         ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(int estimatedSize, CancellationToken cancellationToken = default) {
        if (estimatedSize is 0) {
            return new Dictionary<string, ReadOnlyMemory<byte>>();
        }
        using var file = new FileStream(_path, FileMode.Open);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            await MemoryPackSerializer.DeserializeAsync<Dictionary<string, ReadOnlyMemory<byte>>>(file, cancellationToken: cancellationToken)
             ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override void Serialize(Dictionary<string, ReadOnlyMemory<byte>> dict, int estimatedSize) {
        using var file = new FileStream(_path, FileMode.Create);
        using var buffer = new RentedBufferWriter<byte>(estimatedSize);
        MemoryPackSerializer.Serialize(buffer, dict);
        file.Write(buffer.WrittenSpan);
    }

/// <inheritdoc />
    internal override async ValueTask SerializeAsync(Dictionary<string, ReadOnlyMemory<byte>> dict, int estimatedSize, CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Create);
        await MemoryPackSerializer.SerializeAsync(file, dict, cancellationToken: cancellationToken);
    }
}