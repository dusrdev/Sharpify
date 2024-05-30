namespace Sharpify.Data.Tests;

public class DatabaseIgnoreCaseTests {
    private static Func<string, FactoryResult<Database>> Factory => p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        var database = Database.CreateOrLoad(new() {
            Path = path,
            IgnoreCase = true,
            SerializeOnUpdate = true,
            TriggerUpdateEvents = true,
        });
        return new(path, database);
    };

    private static Func<string, Task<FactoryResult<Database>>> AsyncFactory => async p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        var database = await Database.CreateOrLoadAsync(new() {
            Path = path,
            IgnoreCase = true,
            SerializeOnUpdate = false,
            TriggerUpdateEvents = false,
        });
        return new(path, database);
    };

    [Fact]
    public void SerializeAndDeserialize() {
        using var database = Database.CreateOrLoad(new() {
            Path = Path.GetTempFileName(),
            EncryptionKey = "test",
            IgnoreCase = true,
        });

        database.Upsert("test", new Person("David", 27));
        database.Serialize();
        var length = new FileInfo(database.Config.Path).Length;

        using var database2 = Database.CreateOrLoad(new() {
            Path = database.Config.Path,
            EncryptionKey = "test",
            IgnoreCase = true,
        });

        database2.TryGetValue("TEST", out Person result).Should().BeTrue();
        result.Should().Be(new Person("David", 27));
    }

    [Fact]
    public async Task AsyncSerializeDeserialize() {
        // Arrange
        using var db = await AsyncFactory("");

        // Act
        db.Database.Upsert("test", new Person("David", 27));

        await db.Database.SerializeAsync();

        // Arrange
        using var db2 = await AsyncFactory(db.Path);

        // Assert
        db2.Database.TryGetValue("TEST", out Person result).Should().BeTrue();
        result.Should().Be(new Person("David", 27));

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void Upsert() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.Upsert("test", "test");
        db.Database.Serialize();

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.TryGetString("TEST", out string result).Should().BeTrue();
        result.Should().Be("test");

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void UpsertEncrypted() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.Upsert("test", "test", "enc");
        db.Database.Serialize();

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.TryGetString("TEST", "enc", out string result).Should().BeTrue();
        result.Should().Be("test");

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void UpsertBytes() {
        // Arrange
        using var db = Factory("");

        // Act
        byte[] bytes = [1, 2, 3, 4, 5];
        db.Database.Upsert("test", bytes);

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.TryGetValue("TEST", out byte[] result).Should().BeTrue();
        result.SequenceEqual(bytes).Should().BeTrue();

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
        db2.Database.TryGetValue<Person>("1", out var p2).Should().BeTrue();
        p2.Should().Be(p1);

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void UpsertMany() {
        // Arrange
        using var db = Factory("");

        // Act
        var p1 = new Person("David", 27);
        var p2 = new Person("John", 30);
        db.Database.UpsertMany("1", new []{ p1, p2 });

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.TryGetValues<Person>("1", out var arr).Should().BeTrue();
        arr.Should().ContainInOrder(p1, p2);

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void UpsertJson() {
        // Arrange
        using var db = Factory("");

        // Act
        var p1 = new Color {
            Name = "Red",
            Red = 255,
            Green = 0,
            Blue = 0
        };
        db.Database.Upsert("1", p1, JsonContext.Default.Color);

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.TryGetValue("1", JsonContext.Default.Color, out var p2).Should().BeTrue();
        p2.Should().Be(p1);

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void GeneralFilterTest() {
        using var db = Factory("");

        // Act
        var p1 = new Person("David", 27);
        var d1 = new Dog("Buddy", 5);

        db.Database.CreateMemoryPackFilter<Person>().Upsert("David", p1);
        db.Database.CreateMemoryPackFilter<Dog>().Upsert("Buddy", d1);

        // Arrange
        using var db2 = Factory(db.Path);

        // Assert
        db2.Database.ContainsKey("David").Should().BeFalse();
        db2.Database.ContainsKey("Buddy").Should().BeFalse();
        db.Database.CreateMemoryPackFilter<Person>().TryGetValue("DAVID", out var p2).Should().BeTrue();
        db.Database.CreateMemoryPackFilter<Dog>().TryGetValue("BUDDY", out var d2).Should().BeTrue();
        p2.Should().Be(p1);
        d2.Should().Be(d1);

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
    public void Contains() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.Upsert("test", "test");

        // Assert
        db.Database.ContainsKey("TEST").Should().BeTrue();

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void ContainsFiltered() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.CreateMemoryPackFilter<Person>().Upsert("test", new Person("David", 27));

        // Assert
        db.Database.CreateMemoryPackFilter<Person>().ContainsKey("TEST").Should().BeTrue();

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void Remove() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.Upsert("test", "test");
        db.Database.Remove("test");

        // Assert
        db.Database.ContainsKey("TEST").Should().BeFalse();

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void RemovePredicate() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.Upsert("test", "test");
        db.Database.Remove(key => key == "test");

        // Assert
        db.Database.ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void RemoveFiltered() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.CreateMemoryPackFilter<Person>().Upsert("test", new Person("David", 27));
        db.Database.CreateMemoryPackFilter<Person>().Remove("test");

        // Assert
        db.Database.CreateMemoryPackFilter<Person>().ContainsKey("TEST").Should().BeFalse();

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void RemoveFilteredPredicate() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.CreateMemoryPackFilter<Person>().Upsert("test", new Person("David", 27));
        db.Database.CreateMemoryPackFilter<Person>().Remove(key => key == "test");

        // Assert
        db.Database.CreateMemoryPackFilter<Person>().ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(db.Path);
    }

    [Fact]
    public void Clear() {
        // Arrange
        using var db = Factory("");

        // Act
        db.Database.Upsert("test", "test");
        db.Database.Clear();

        // Assert
        db.Database.ContainsKey("TEST").Should().BeFalse();

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
            _database.Upsert(item.ToString(), rnd.ToString());
            return Task.CompletedTask;
        }
    }
}