namespace Sharpify.Tests;

public readonly record struct TempFile : IDisposable {
	public string _path { get; }

	public TempFile() {
		_path = Path.GetTempFileName();
	}

	public static implicit operator string(TempFile file) => file._path;

	public void Dispose() {
		if (File.Exists(_path)) {
			File.Delete(_path);
		}
	}
}