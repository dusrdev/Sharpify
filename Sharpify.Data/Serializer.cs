using MemoryPack;

namespace Sharpify.Data;

/// <summary>
/// A serializer for a database without encryption and case sensitive keys
/// </summary>
internal class Serializer : DatabaseSerializer {
    internal Serializer(string path) : base(path) {
    }

/// <inheritdoc />
    internal override Dictionary<string, ReadOnlyMemory<byte>> Deserialize() {
        ReadOnlySpan<byte> bin = File.ReadAllBytes(_path);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            MemoryPackSerializer.Deserialize<Dictionary<string, ReadOnlyMemory<byte>>>(bin)
         ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override async ValueTask<Dictionary<string, ReadOnlyMemory<byte>>> DeserializeAsync(CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Open);
        Dictionary<string, ReadOnlyMemory<byte>> dict =
            await MemoryPackSerializer.DeserializeAsync<Dictionary<string, ReadOnlyMemory<byte>>>(file, cancellationToken: cancellationToken)
             ?? new Dictionary<string, ReadOnlyMemory<byte>>();
        return dict;
    }

/// <inheritdoc />
    internal override void Serialize(Dictionary<string, ReadOnlyMemory<byte>> dict) {
        using var file = new FileStream(_path, FileMode.Create);
        ReadOnlySpan<byte> buffer = MemoryPackSerializer.Serialize(dict);
        file.Write(buffer);
    }

/// <inheritdoc />
    internal override async ValueTask SerializeAsync(Dictionary<string, ReadOnlyMemory<byte>> dict, CancellationToken cancellationToken = default) {
        using var file = new FileStream(_path, FileMode.Create);
        await MemoryPackSerializer.SerializeAsync(file, dict, cancellationToken: cancellationToken);
    }
}