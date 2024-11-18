namespace Sharpify.Data.Tests;

public class DatabaseEncryptedTests {
    private static Func<string, (Database, string)> Factory => p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        return (Database.CreateOrLoad(new() {
            Path = path,
            EncryptionKey = "test",
            SerializeOnUpdate = true,
            TriggerUpdateEvents = true,
        }), path);
    };

    private static Func<string, ValueTask<(Database, string)>> AsyncFactory => async p => {
        var path = p.Length is 0 ?
                    Path.GetTempFileName()
                    : p;
        return (await Database.CreateOrLoadAsync(new() {
            Path = path,
            EncryptionKey = "test",
            SerializeOnUpdate = false,
            TriggerUpdateEvents = false,
        }), path);
    };

    [Fact]
    public void SerializeAndDeserialize() {
        var database = Database.CreateOrLoad(new() {
            Path = Path.GetTempFileName(),
            EncryptionKey = "test",
        });

        database.Upsert("test", new Person("David", 27));
        database.Serialize();

        var database2 = Database.CreateOrLoad(new() {
            Path = database.Config.Path,
            EncryptionKey = "test",
        });

        database2.TryGetValue("test", out Person result).Should().BeTrue();
        result.Should().Be(new Person("David", 27));
    }

    [Fact]
    public async Task AsyncSerializeDeserialize() {
        // Arrange
        var (db, path) = await AsyncFactory(string.Empty);

        // Act
        db.Upsert("test", new Person("David", 27));

        await db.SerializeAsync();

        // Arrange
        var (db2, _) = await AsyncFactory(path);

        // Assert
        db2.TryGetValue("test", out Person result).Should().BeTrue();
        result.Should().Be(new Person("David", 27));

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Upsert() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.Upsert("test", "test");
        db.Serialize();

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.TryGetString("test", out string result).Should().BeTrue();
        result.Should().Be("test");

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void UpsertEncrypted() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.Upsert("test", "test", "enc");
        db.Serialize();

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.TryGetString("test", "enc", out string result).Should().BeTrue();
        result.Should().Be("test");

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void UpsertBytes() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        var bytes = new byte[] { 1, 2, 3, 4, 5 };
        db.Upsert("test", bytes);

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.TryGetValue("test", out var result).Should().BeTrue();
        result.Span.SequenceEqual(bytes).Should().BeTrue();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void UpsertMemoryPackable() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        var p1 = new Person("David", 27);
        db.Upsert("1", p1);

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.TryGetValue<Person>("1", out var p2).Should().BeTrue();
        p2.Should().Be(p1);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void UpsertMany() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        var p1 = new Person("David", 27);
        var p2 = new Person("John", 30);
        db.UpsertMany("1", [p1, p2]);

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.TryGetValues<Person>("1", out var arr).Should().BeTrue();
        arr.Should().ContainInOrder(p1, p2);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void UpsertJson() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        var p1 = new Color {
            Name = "Red",
            Red = 255,
            Green = 0,
            Blue = 0
        };
        db.Upsert("1", p1, JsonContext.Default.Color);

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.TryGetValue("1", JsonContext.Default.Color, out var p2).Should().BeTrue();
        p2.Should().Be(p1);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void GeneralFilterTest() {
        var (db, path) = Factory("");

        // Act
        var p1 = new Person("David", 27);
        var d1 = new Dog("Buddy", 5);

        db.CreateMemoryPackFilter<Person>().Upsert("David", p1);
        db.CreateMemoryPackFilter<Dog>().Upsert("Buddy", d1);

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.ContainsKey("David").Should().BeFalse();
        db2.ContainsKey("Buddy").Should().BeFalse();
        db.CreateMemoryPackFilter<Person>().TryGetValue("David", out var p2).Should().BeTrue();
        db.CreateMemoryPackFilter<Dog>().TryGetValue("Buddy", out var d2).Should().BeTrue();
        p2.Should().Be(p1);
        d2.Should().Be(d1);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public async Task UpsertConcurrently() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        var items = Enumerable.Range(0, 100).ToArray();
        var test = new ConcurrentDatabaseTest(db);
        await items.ForAllAsync(test);

        // Arrange
        var (db2, _) = Factory(path);

        // Assert
        db2.Count.Should().Be(100);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Contains() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.Upsert("test", "test");

        // Assert
        db.ContainsKey("test").Should().BeTrue();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void ContainsFiltered() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.CreateMemoryPackFilter<Person>().Upsert("test", new Person("David", 27));

        // Assert
        db.CreateMemoryPackFilter<Person>().ContainsKey("test").Should().BeTrue();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Remove() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.Upsert("test", "test");
        db.Remove("test");

        // Assert
        db.ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void RemovePredicate() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.Upsert("test", "test");
        db.Remove(key => key == "test");

        // Assert
        db.ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void RemoveFiltered() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.CreateMemoryPackFilter<Person>().Upsert("test", new Person("David", 27));
        db.CreateMemoryPackFilter<Person>().Remove("test");

        // Assert
        db.CreateMemoryPackFilter<Person>().ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void RemoveFilteredPredicate() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.CreateMemoryPackFilter<Person>().Upsert("test", new Person("David", 27));
        db.CreateMemoryPackFilter<Person>().Remove(key => key == "test");

        // Assert
        db.CreateMemoryPackFilter<Person>().ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Clear() {
        // Arrange
        var (db, path) = Factory("");

        // Act
        db.Upsert("test", "test");
        db.Clear();

        // Assert
        db.ContainsKey("test").Should().BeFalse();

        // Cleanup
        File.Delete(path);
    }
}