
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify.Tests;

public class MonitoredSerializableObjectTests {
    private static readonly string _basePath = Utils.Env.GetBaseDirectory();

    [Fact]
    public void Constructor_Throws_When_Directory_Does_Not_Exist() {
        // Arrange
        var path = "nonexistent/file.json";
        var action = () => new MonitoredSerializableObject<Configuration>(path);

        // Act & Assert
        action.Should().Throw<IOException>();
    }

    [Fact]
    public void Constructor_Throws_When_Filename_Is_Invalid() {
        // Arrange
        var action = () => new MonitoredSerializableObject<Configuration>(_basePath); // no filename

        // Act & Assert
        action.Should().Throw<IOException>();
    }

    [Fact]
    public void Constructor_Creates_File_When_File_Does_Not_Exist() {
        // Arrange
        var path = Path.Join(_basePath, "file.json");
        var action = () => new MonitoredSerializableObject<Configuration>(path);

        // Act
        action.Should().NotThrow();

        // Assert
        File.Exists(path).Should().BeTrue();

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Constructor_Deserializes_File_When_File_Exists() {
        // Arrange
        var path = Path.GetTempFileName();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        var action = () => new MonitoredSerializableObject<Configuration>(path, config);

        // Act
        action.Should().NotThrow();
        // Assert
        File.Exists(path).Should().BeTrue();
        // Act
        using var obj = new MonitoredSerializableObject<Configuration>(path);
        // Assert
        obj.Value.Should().BeEquivalentTo(config);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Modify_SerializesProperly() {
        // Arrange
        var path = Path.GetTempFileName();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(path, config);
        const string newName = "Jane Doe";

        // Act
        obj.Modify(c => c with { Name = newName });
        // Assert
        using var obj2 = new MonitoredSerializableObject<Configuration>(path);
        obj.Value.Name.Should().BeEquivalentTo(newName);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void Modify_FiresEventOnce_WithProperArgs() {
        // Arrange
        var path = Path.GetTempFileName();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(path, config);
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

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void OnFileChanged_DoesntChangeWhenFileIsEmpty() {
        // Arrange
        var path = Path.GetTempFileName();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(path, config);
        int count = 0;
        Configuration lastValue = default;
        obj.OnChanged += (sender, e) => {
            count++;
            lastValue = e.Value;
        };

        // Act
        File.WriteAllText(path, "");
        // Assert
        count.Should().Be(0);

        // Cleanup
        File.Delete(path);
    }

    [Fact]
    public void OnFileChanged_DoesntChangeWhenFileIsInvalid() {
        // Arrange
        var path = Path.GetTempFileName();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(path, config);
        int count = 0;
        Configuration lastValue = default;
        obj.OnChanged += (sender, e) => {
            count++;
            lastValue = e.Value;
        };

        // Act
        File.WriteAllText(path, "invalid json");
        // Assert
        count.Should().Be(0);

        // Cleanup
        File.Delete(path);
    }

    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    [Fact]
    public void OnFileChanged_ChangesWhenFileIsValid() {
        // Arrange
        var path = Path.GetTempFileName();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(path, config);
        // Assert
        obj.OnChanged += (sender, e) => {
            e.Value.Should().Be("Jane");
        };

        // Act
        File.WriteAllText(path, JsonSerializer.Serialize(config with { Name = "Jane" }, Options));

        // Cleanup
        File.Delete(path);
    }

    private record struct Configuration {
        public string Name { get; set; }
        public int Age { get; set; }
    }
}