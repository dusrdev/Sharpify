using MemoryPack;

namespace Sharpify.Data.Tests;

[MemoryPackable]
public readonly partial record struct Person(string Name, int Age);