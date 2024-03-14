using System.Collections.Concurrent;

namespace Sharpify.Data;

public sealed partial class Database : IDisposable {
    private readonly ConcurrentDictionary<string, bool> _atomicLocks = new();

    private void AcquireAtomicLock(string key, bool lockValue) {
        if (lockValue) {
            _atomicLocks.TryAdd(key, true);
            return;
        }
        if (!_atomicLocks.ContainsKey(key)) {
            return;
        }
        SpinWait.SpinUntil(() => !_atomicLocks.TryGetValue(key, out _));
    }

    private void ReleaseAtomicLock(string key) {
        if (!_atomicLocks.ContainsKey(key)) {
            return;
        }
        _atomicLocks.TryRemove(key, out _);
    }
}