using System.Runtime.CompilerServices;

namespace Sharpify.Data;

internal static class Extensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetFileSize(string path) {
		var info = new FileInfo(path);
		return unchecked((int)info.Length);
	}

	internal static byte[]? FastCopy(this byte[]? source) {
		if (source is null) {
			return null;
		}
		if (source.Length is 0) {
			return Array.Empty<byte>();
		}
		var dest = GC.AllocateUninitializedArray<byte>(source.Length);
		source.AsSpan().CopyTo(dest);
		return dest;
	}

	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static int GetEstimatedSize(this KeyValuePair<string, byte[]?> kvp) {
		var valLength = kvp.Value is null ? 10 : kvp.Value.Length;
		return kvp.Key.Length * sizeof(char) + valLength;
	}
}