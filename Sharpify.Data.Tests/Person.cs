using MemoryPack;

namespace Sharpify.Data.Tests;

[MemoryPackable]
public readonly partial record struct Person(string Name, int Age);

[MemoryPackable]
public readonly partial record struct Dog(string Name, int Age);