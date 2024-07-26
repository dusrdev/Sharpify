using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class TestLocalPersistentDictionary : LocalPersistentDictionary {
    private volatile int _serializedCount;

    public TestLocalPersistentDictionary(string path) : base(path) {
    }

    public int SerializedCount => _serializedCount;

    public override async Task SerializeDictionaryAsync() {
        Interlocked.Increment(ref _serializedCount);
        await base.SerializeDictionaryAsync();
    }
}
