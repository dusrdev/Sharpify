using System.Text.Json.Serialization;

using MemoryPack;

namespace Sharpify.Data.Tests;

[MemoryPackable]
public readonly partial record struct Person(string Name, int Age);

[MemoryPackable]
public readonly partial record struct Dog(string Name, int Age);

public record Color {
	public string Name { get; set; } = "";
	public byte Red { get; set; }
	public byte Green { get; set; }
	public byte Blue { get; set; }
}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(Color))]
public partial class JsonContext : JsonSerializerContext { }