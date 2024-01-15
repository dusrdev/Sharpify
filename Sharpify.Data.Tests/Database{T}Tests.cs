using MemoryPack;

namespace Sharpify.Data.Tests;

public class DatabaseTTests {
    private static Func<string, FactoryResult<Database<Person>>> Factory => p => {
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
        return new(path, database);
    };

    [Fact]
    public void Upsert() {
        // Arrange
        using var db = Factory("");

        // Act
        var p1 = new Person("David", 27);
        db.Database.Upsert("1", p1);

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database["1"].Should().Be(p1);

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public async Task UpsertConcurrently() {
        // Arrange
        using var db = Factory("");

        // Act
        var items = Enumerable.Range(0, 100).ToArray();
        var test = new ConcurrentTest(db.Database);
        await items.Concurrent().ForEachAsync(test);

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.Count.Should().Be(100);

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void Remove() {
        // Arrange
        using var db = Factory("");

        // Act
        var p1 = new Person("David", 27);
        db.Database.Upsert("1", p1);
        db.Database.Remove("1");

        // Assert
        db.Database.ContainsKey("1").Should().BeFalse();

        // Cleanup
        File.Delete(db.Path);
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