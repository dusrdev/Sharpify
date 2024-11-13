using System.Text.Json.Serialization;

namespace Sharpify.Data.Tests;

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Color))]
public partial class JsonContext : JsonSerializerContext { }