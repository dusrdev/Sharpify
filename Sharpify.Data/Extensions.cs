using System.Runtime.CompilerServices;

namespace Sharpify.Data;

internal static class Extensions {
	/// <summary>
	/// Gets the size of the file.
	/// </summary>
	/// <param name="path"></param>
	/// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
	internal static int GetFileSize(string path) {
		var info = new FileInfo(path);
		return unchecked((int)info.Length);
	}

	/// <summary>
	/// Copies the specified source array.
	/// </summary>
	/// <param name="source"></param>
	/// <returns></returns>
	internal static byte[] FastCopy(this byte[] source) {
		if (source.Length is 0) {
			return Array.Empty<byte>();
		}
		var dest = GC.AllocateUninitializedArray<byte>(source.Length);
		source.AsSpan().CopyTo(dest);
		return dest;
	}

	/// <summary>
	/// Gets the estimated size of the key-value pair.
	/// </summary>
	/// <param name="kvp"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveOptimization)]
	internal static int GetEstimatedSize(this KeyValuePair<string, byte[]> kvp)
		=> GetEstimatedSize(kvp.Key, kvp.Value);


	/// <summary>
	/// Gets the estimated size of a key-value pair.
	/// </summary>
	/// <param name="key"></param>
	/// <param name="value"></param>
	/// <returns></returns>
	[MethodImpl(MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining)]
	public static int GetEstimatedSize(ReadOnlySpan<char> key, ReadOnlySpan<byte> value)
		=> key.Length * sizeof(char) + value.Length;
}