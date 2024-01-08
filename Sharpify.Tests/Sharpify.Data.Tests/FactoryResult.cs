namespace Sharpify.Tests.Sharpify.Data.Tests;

public record FactoryResult<T>(string Path, T Database) : IDisposable where T : IDisposable {
    public void Dispose() => Database.Dispose();
}