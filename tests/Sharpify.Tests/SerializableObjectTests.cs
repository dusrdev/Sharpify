
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
        action.Should().Throw<IOException>();

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Constructor_Creates_File_When_File_Does_Not_Exist() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var action = () => new MonitoredSerializableObject<Configuration>(file, JsonContext.Default.Configuration);

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
        var action = () => new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);

        // Act
        action.Should().NotThrow();
        // Assert
        File.Exists(file).Should().BeTrue();
        // Act
        using var obj = new MonitoredSerializableObject<Configuration>(file, JsonContext.Default.Configuration);
        // Assert
        obj!.Value.Should().BeEquivalentTo(config);

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Modify_SerializesProperly() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);
        const string newName = "Jane Doe";

        // Act
        obj.Modify(c => c with { Name = newName });
        // Assert
        using var obj2 = new MonitoredSerializableObject<Configuration>(file, JsonContext.Default.Configuration);
        obj.Value.Name.Should().BeEquivalentTo(newName);

        // Cleanup
        await file.DeleteAsync();
    }

    [Fact]
    public async Task Modify_FiresEventOnce_WithProperArgs() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);
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
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);
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
        Configuration lastValue = default;
        obj.OnChanged += (sender, e) => {
            count++;
            lastValue = e.Value;
        };

        // Act
        File.WriteAllText(file, "invalid json");
        // Assert
        count.Should().Be(0);

        // Cleanup
        await file.DeleteAsync();
    }

    private static readonly JsonSerializerOptions Options = new() {
        WriteIndented = true,
        IncludeFields = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    [Fact]
    public async Task OnFileChanged_ChangesWhenFileIsValid() {
        // Arrange
        var file = await TempFile.CreateAsync();
        var config = new Configuration { Name = "John Doe", Age = 42 };
        using var obj = new MonitoredSerializableObject<Configuration>(file, config, JsonContext.Default.Configuration);
        // Assert
        obj.OnChanged += (sender, e) => e.Value.Name.Should().Be("Jane");

        // Act
        File.WriteAllText(file, JsonSerializer.Serialize(config with { Name = "Jane" }, Options));

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