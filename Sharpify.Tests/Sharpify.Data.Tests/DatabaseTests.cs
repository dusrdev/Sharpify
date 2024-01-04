using Sharpify.Data;

namespace Sharpify.Tests.Sharpify.Data.Tests;

public class DatabaseTests {
    private static Func<string, (string, Database)> Factory => p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        var database = Database.Create(new() {
            Path = path,
            EncryptionKey = "test",
            Options = DatabaseOptions.SerializeOnUpdate | DatabaseOptions.TriggerUpdateEvents
        });
        return (path, database);
    };

    [Fact]
    public void Upsert() {
        // Arrange
        var (path, database) = Factory("");

        // Act
        database.UpsertAsString("test", "test");

        // Arrange
        var (_, database2) = Factory(path);

        // Assert
        database2.GetAsString("test").Should().Be("test");

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void UpsertMemoryPackable() {
        // Arrange
        var (path, database) = Factory("");

        // Act
        var p1 = new Person("David", 27);
        database.Upsert("1", p1);

        // Arrange
        var (_, database2) = Factory(path);

        // Assert
        var p2 = database2.Get<Person>("1");
        p2.Should().Be(p1);

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
        var (path, database) = Factory("");
        database.UpsertAsString("test", "test");
        database.Remove("test");
        database.ContainsKey("test").Should().BeFalse();
    }

    private class ConcurrentTest : IAsyncAction<int> {
        private readonly Database _database;

        public ConcurrentTest(Database database) {
            _database = database;
        }

        public Task InvokeAsync(int item) {
            var rnd = Random.Shared.Next(10_000, 200_000);
            _database.UpsertAsString(item.ToString(), rnd.ToString());
            return Task.CompletedTask;
        }
    }
}