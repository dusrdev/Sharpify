using MemoryPack;

namespace Sharpify.Data.Tests;

public record Color {
	public string Name { get; set; } = "";
	public byte Red { get; set; }
	public byte Green { get; set; }
	public byte Blue { get; set; }
}