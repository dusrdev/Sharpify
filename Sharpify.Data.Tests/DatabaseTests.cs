namespace Sharpify.Data.Tests;

public class DatabaseTests {
    private static Func<string, FactoryResult<Database>> Factory => p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        var database = Database.Create(new() {
            Path = path,
            EncryptionKey = "test",
            Options = DatabaseOptions.SerializeOnUpdate | DatabaseOptions.TriggerUpdateEvents
        });
        return new(path, database);
    };

    [Fact]
    public void Upsert() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.UpsertAsString("test", "test");

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.GetAsString("test").Should().Be("test");

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void UpsertMemoryPackable() {
        // Arrange
        using var db = Factory("");

        // Act
        var p1 = new Person("David", 27);
        db.Database.Upsert("1", p1);

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        var p2 = db2.Database.Get<Person>("1");
        p2.Should().Be(p1);

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
        db.Database.UpsertAsString("test", "test");
        db.Database.Remove("test");

        // Assert
        db.Database.ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(db.Path);
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