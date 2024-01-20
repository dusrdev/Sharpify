using System.Runtime.CompilerServices;

namespace Sharpify.Data;

internal static class Extensions {
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetFileSize(string path) {
		var info = new FileInfo(path);
		return unchecked((int)info.Length);
	}
}