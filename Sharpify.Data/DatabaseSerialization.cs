using System.Runtime.CompilerServices;

namespace Sharpify.Data;

public sealed partial class Database : IDisposable {
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    private void EnsureUpsertsAreFinished() {
        if (!Config.SerializeOnUpdate) {
            while (_queue.TryDequeue(out var kvp)) {
                _data[kvp.Key] = kvp.Value;
                var estimatedSize = kvp.GetEstimatedSize();
                Interlocked.Add(ref _estimatedSize, estimatedSize);
            }
        }
    }

    /// <summary>
    /// Saves the database to the hard disk.
    /// </summary>
    public void Serialize() {
        EnsureUpsertsAreFinished();
        _serializer.Serialize(_data, GetOverestimatedSize());
    }

    /// <summary>
    /// Saves the database to the hard disk asynchronously.
    /// </summary>
    public ValueTask SerializeAsync(CancellationToken cancellationToken = default) {
        EnsureUpsertsAreFinished();
        return _serializer.SerializeAsync(_data, GetOverestimatedSize(), cancellationToken);
    }
}