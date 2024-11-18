namespace Sharpify.Data.Tests;

internal class ConcurrentDatabaseTest : IAsyncAction<int> {
	private readonly Database _database;

	public ConcurrentDatabaseTest(Database database) {
		_database = database;
	}

	public Task InvokeAsync(int item, CancellationToken token = default) {
		var rnd = Random.Shared.Next(10_000, 200_000);
		_database.Upsert(item.ToString(), rnd.ToString());
		return Task.CompletedTask;
	}
}