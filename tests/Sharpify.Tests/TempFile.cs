namespace Sharpify.Tests;

public record TempFile {
	public string _path { get; }
	private int _retries = 5;

	public TempFile() {
		_path = Path.GetTempFileName();
	}

	public static implicit operator string(TempFile file) => file._path;

	public async ValueTask DeleteAsync() {
		if (!File.Exists(_path)) {
			return;
		}

	delete:
		try {
			File.Delete(_path);
		} catch {
			if (_retries++ < 5) {
				await Task.Delay(100);
				goto delete;
			} else {
				throw;
			}
		}
	}
}