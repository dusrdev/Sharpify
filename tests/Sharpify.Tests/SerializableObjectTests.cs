
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Sharpify.Tests;

[Collection("SerializableObjectTests")]
public class SerializableObjectTests {
    [Fact]
    public async Task Constructor_Throws_When_Filename_Is_Invalid() {
        var file = await TempFile.CreateAsync();
        var dir = Path.GetDirectoryName(file)!;
        // Arrange
        var action = () => new MonitoredSerializableObject<Configuration>(dir, JsonContext.Default.Configuration); // no filename

        // Act & Assert
        action.Should().Throw<ArgumentException>();

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Constructor_Creates_File_When_File_Does_Not_Exist() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var action = () => new MonitoredSerializableObject<Configuration>(file.Path, JsonContext.Default.Configuration);

        // Act
        action.Should().NotThrow();

        // Assert
        File.Exists(file).Should().BeTrue();
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Constructor_Deserializes_File_When_File_Exists() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        var action = () => new MonitoredSerializableObject<Configuration>(file.Path, config, JsonContext.Default.Configuration);

        // Act
        action.Should().NotThrow();
        // Assert
        File.Exists(file).Should().BeTrue();
        // Act
        using var obj = new MonitoredSerializableObject<Configuration>(file.Path, JsonContext.Default.Configuration);
        // Assert
        obj.Value.Should().BeEquivalentTo(config);

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Modify_SerializesProperly() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file.Path, config, JsonContext.Default.Configuration);
        const string newName = "Jane Doe";

        // Act
        obj.Modify(c => c with { Name = newName });
        // Assert
        obj.Value.Name.Should().Be(newName);
        using var obj2 = new MonitoredSerializableObject<Configuration>(file.Path, JsonContext.Default.Configuration);
        obj2.Value.Name.Should().BeEquivalentTo(newName);

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Modify_FiresEventOnce_WithProperArgs() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file.Path, config, JsonContext.Default.Configuration);
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
        await file.DeleteAsync();
    }

    [Fact]
    public async Task OnFileChanged_DoesntChangeWhenFileIsEmpty() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file.Path, config, JsonContext.Default.Configuration);
        int count = 0;
        obj.OnChanged += (sender, e) => {
            Interlocked.Increment(ref count);
        };

        // Act
        await File.WriteAllTextAsync(file, "");
        // Assert
        count.Should().Be(0);

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task OnFileChanged_DoesntChangeWhenFileIsInvalid() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);
        int count = 0;
        obj.OnChanged += (sender, e) => {
            Interlocked.Increment(ref count);
        };

        // Act
        await File.WriteAllTextAsync(file, "invalid json");
        // Assert
        count.Should().Be(0);

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task OnFileChanged_ChangesWhenFileIsValid() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);
        // Assert
        obj.OnChanged += (sender, e) => e.Value.Name.Should().Be("Jane");

        // Act
        await File.WriteAllTextAsync(file, JsonSerializer.Serialize(config with { Name = "Jane" }, JsonContext.Default.Configuration));

        // Cleanup
        await file.DeleteAsync();
    }
}
internal record struct Configuration {
    public string Name { get; set; }
    public int Age { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Configuration))]
internal partial class JsonContext : JsonSerializerContext { }