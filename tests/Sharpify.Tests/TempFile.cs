namespace Sharpify.Tests;

public record TempFile {
	public string Path { get; }
	private const int Retries = 5;

	public static async Task<TempFile> CreateAsync() {
		int retries = Retries;
	create:
		try {
			return new TempFile();
		} catch {
			if (Interlocked.Decrement(ref retries) < 0) {
				await Task.Delay(100);
				goto create;
			} else {
				throw;
			}
		}
	}

	private TempFile() {
		Path = Utils.Env.PathInBaseDirectory(Random.Shared.Next(1000000, 9999999).ToString());
		using var _ = File.Create(Path);
	}

	public static implicit operator string(TempFile file) => file.Path;

	public async ValueTask DeleteAsync() {
		if (!File.Exists(Path)) {
			return;
		}

		int retries = Retries;

	delete:
		try {
			File.Delete(Path);
		} catch {
			if (Interlocked.Decrement(ref retries) < 0) {
				await Task.Delay(100);
				goto delete;
			} else {
				throw;
			}
		}
	}
}