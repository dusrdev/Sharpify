
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify.Tests;

[Collection("SerializableObjectTests")]
public class SerializableObjectTests {
    [Fact]
    public void Constructor_Throws_When_Filename_Is_Invalid() {
        using var file = new TempFile();
        var dir = Path.GetDirectoryName(file)!;
        // Arrange
        var action = () => new MonitoredSerializableObject<Configuration>(dir, JsonContext.Default); // no filename

        // Act & Assert
        action.Should().Throw<IOException>();
    }

    [Fact]
    public void Constructor_Creates_File_When_File_Does_Not_Exist() {
        // Arrange
        using var file = new TempFile();
        var action = () => new MonitoredSerializableObject<Configuration>(file, JsonContext.Default);

        // Act
        action.Should().NotThrow();

        // Assert
        File.Exists(file).Should().BeTrue();
    }

    [Fact]
    public void Constructor_Deserializes_File_When_File_Exists() {
        // Arrange
        using var file = new TempFile();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        var action = () => new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default);

        // Act
        action.Should().NotThrow();
        // Assert
        File.Exists(file).Should().BeTrue();
        // Act
        using var obj = new MonitoredSerializableObject<Configuration>(file, JsonContext.Default);
        // Assert
        obj!.Value.Should().BeEquivalentTo(config);
    }

    [Fact]
    public void Modify_SerializesProperly() {
        // Arrange
        using var file = new TempFile();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default);
        const string newName = "Jane Doe";

        // Act
        obj.Modify(c => c with { Name = newName });
        // Assert
        using var obj2 = new MonitoredSerializableObject<Configuration>(file, JsonContext.Default);
        obj.Value.Name.Should().BeEquivalentTo(newName);
    }

    [Fact]
    public void Modify_FiresEventOnce_WithProperArgs() {
        // Arrange
        using var file = new TempFile();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default);
        const string newName = "Jane Doe";
        int count = 0;
        Configuration lastValue = default;
        obj.OnChanged += (sender, e) => {
            count++;
            lastValue = e.Value;
        };

        // Act
        obj.Modify(c => c with { Name = newName });
        // Assert
        count.Should().Be(1);
        lastValue.Name.Should().Be(newName);
    }

    [Fact]
    public void OnFileChanged_DoesntChangeWhenFileIsEmpty() {
        // Arrange
        using var file = new TempFile();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default);
        int count = 0;
        Configuration lastValue = default;
        obj.OnChanged += (sender, e) => {
            count++;
            lastValue = e.Value;
        };

        // Act
        File.WriteAllText(file, "");
        // Assert
        count.Should().Be(0);
    }

    [Fact]
    public void OnFileChanged_DoesntChangeWhenFileIsInvalid() {
        // Arrange
        using var file = new TempFile();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default);
        int count = 0;
        Configuration lastValue = default;
        obj.OnChanged += (sender, e) => {
            count++;
            lastValue = e.Value;
        };

        // Act
        File.WriteAllText(file, "invalid json");
        // Assert
        count.Should().Be(0);
    }

    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    [Fact]
    public void OnFileChanged_ChangesWhenFileIsValid() {
        // Arrange
        using var file = new TempFile();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default);
        // Assert
        obj.OnChanged += (sender, e) => {
            e.Value.Should().Be("Jane");
        };

        // Act
        File.WriteAllText(file, JsonSerializer.Serialize(config with { Name = "Jane" }, Options));
    }
}
internal record struct Configuration {
    public string Name { get; set; }
    public int Age { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Configuration))]
internal partial class JsonContext : JsonSerializerContext { }