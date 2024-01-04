using MemoryPack;

namespace Sharpify.Tests.Sharpify.Data.Tests;

[MemoryPackable]
public readonly partial record struct Person(string Name, int Age);