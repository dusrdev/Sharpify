using MemoryPack;

using Sharpify.Data;

namespace Sharpify.Tests.Sharpify.Data.Tests;

public class DatabaseTTests {
    private static Func<string, (string, Database<Person>)> Factory => p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        var database = Database<Person>.Create(new() {
            Path = path,
            EncryptionKey = "test",
            Options = DatabaseOptions.SerializeOnUpdate | DatabaseOptions.TriggerUpdateEvents,
            ToByteArray = static p => MemoryPackSerializer.Serialize(p),
            ToT = static b => MemoryPackSerializer.Deserialize<Person>(b)
        });
        return (path, database);
    };

    [Fact]
    public void Upsert() {
        // Arrange
        var (path, database) = Factory("");

        // Act
        var p1 = new Person("David", 27);
        database.Upsert("1", p1);

        // Arrange
        var (_, database2) = Factory(path);

        // Assert
        database2["1"].Should().Be(p1);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public async Task UpsertConcurrently() {
        // Arrange
        var (path, database) = Factory("");

        // Act
        var items = Enumerable.Range(0, 100).ToArray();
        var test = new ConcurrentTest(database);
        await items.Concurrent().ForEachAsync(test);

        // Arrange
        var (_, database2) = Factory(path);

        // Assert
        database2.Count.Should().Be(100);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Remove() {
        // Arrange
        var (path, database) = Factory("");

        // Act
        var p1 = new Person("David", 27);
        database.Upsert("1", p1);
        database.Remove("1");

        // Assert
        database.ContainsKey("1").Should().BeFalse();

        // Cleanup
        File.Delete(path);
    }

    private class ConcurrentTest : IAsyncAction<int> {
        private readonly Database<Person> _database;

        public ConcurrentTest(Database<Person> database) {
            _database = database;
        }

        public Task InvokeAsync(int item) {
            var rnd = Random.Shared.Next(10_000, 200_000);
            var key = item.ToString();
            _database[key] = new Person(key, rnd);
            return Task.CompletedTask;
        }
    }
}