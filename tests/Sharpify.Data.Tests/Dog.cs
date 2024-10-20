using System.Text.Json.Serialization;

using MemoryPack;

namespace Sharpify.Data.Tests;

[MemoryPackable]
public readonly partial record struct Dog(string Name, int Age);