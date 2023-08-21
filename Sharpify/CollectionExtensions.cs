using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Sharpify;

public static partial class Extensions {
    /// <summary>
    /// Returns the span of a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    public static Span<T> AsSpan<T>(this List<T> list) => CollectionsMarshal.AsSpan(list);

    /// <summary>
    /// Gets either a ref to a <typeparamref name="TValue"/> in the <see cref="Dictionary{TKey, TValue}"/> or a ref null if it does not exist in the <paramref name="dictionary"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the ref to <typeparamref name="TValue"/> from.</param>
    /// <param name="key">The key used for lookup.</param>
    /// <remarks>
    /// Items should not be added or removed from the <see cref="Dictionary{TKey, TValue}"/> while the ref <typeparamref name="TValue"/> is in use.
    /// The ref null can be detected using <see cref="Unsafe.IsNullRef{T}(ref T)"/>
    /// </remarks>
    public static ref TValue GetValueRefOrNullRef<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key) where TKey : notnull {
        return ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);
    }

    /// <summary>
    /// Gets a ref to a <typeparamref name="TValue"/> in the <see cref="Dictionary{TKey, TValue}"/>, adding a new entry with a default value if it does not exist in the <paramref name="dictionary"/>.
    /// </summary>
    /// <param name="dictionary">The dictionary to get the ref to <typeparamref name="TValue"/> from.</param>
    /// <param name="key">The key used for lookup.</param>
    /// <param name="exists">Whether or not a new entry for the given key was added to the dictionary.</param>
    /// <remarks>
    /// Items should not be added to or removed from the <see cref="Dictionary{TKey, TValue}"/> while the ref <typeparamref name="TValue"/> is in use.
    /// </remarks>
    public static ref TValue? GetValueRefOrAddDefault<TKey, TValue>(
        this Dictionary<TKey, TValue> dictionary,
        TKey key,
        out bool exists) where TKey : notnull {
        return ref CollectionsMarshal.GetValueRefOrAddDefault(dictionary, key, out exists);
    }

    /// <summary>
    /// Returns a new array with the elements sorted using the default comparer for the element type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer"></param>
    /// <remarks>
    /// If you are using a built-in type you can specify the <see cref="Comparer{T}.Default"/>
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static T[] PureSort<T>(this T[] source, IComparer<T> comparer) {
        if (source.Length <= 1) {
            return source;
        }
        var newArr = new T[source.Length];
        Array.Copy(source, newArr, source.Length);
        Array.Sort(newArr, comparer);
        return newArr;
    }

    /// <summary>
    /// Returns a new list with the elements sorted using the default comparer for the element type.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="source"></param>
    /// <param name="comparer"></param>
    /// <remarks>
    /// If you are using a built-in type you can specify the <see cref="Comparer{T}.Default"/>
    /// </remarks>
    [Pure]
    [MethodImpl(MethodImplOptions.AggressiveOptimization)]
    public static List<T> PureSort<T>(this IEnumerable<T> source, IComparer<T> comparer) {
        var list = new List<T>(source);
        if (list.Count <= 1) {
            return list;
        }
        list.Sort(comparer);
        return list;
    }

    /// <summary>
    /// Removes duplicates from a list. If <paramref name="isSorted"/> is true, the list is assumed to be sorted and duplicates are removed in one iteration without allocating an additional collection. Otherwise, a HashSet is used to remove duplicates in one iteration. An optional <paramref name="comparer"/> can be provided to compare elements for equality.
    /// </summary>
    /// <typeparam name="T">The type of elements in the list.</typeparam>
    /// <param name="list">The list to remove duplicates from.</param>
    /// <param name="isSorted">Whether the list is sorted. Default is false.</param>
    /// <param name="comparer">An optional comparer to use for comparing elements for equality. Default is null.</param>
    public static void RemoveDuplicates<T>(this List<T> list, bool isSorted = false, IEqualityComparer<T>? comparer = null) {
        if (isSorted) {
            list.RemoveDuplicatesSorted(comparer);
            return;
        }
        list.RemoveDuplicatesNotSorted(comparer);
    }

    // Removes duplicates from a sorted list in 1 iteration
    private static void RemoveDuplicatesSorted<T>(this List<T> list, IEqualityComparer<T>? comparer) {
        if (list is { Count: <= 1 }) {
            return;
        }
        comparer ??= EqualityComparer<T>.Default;
        var current = list[0];
        int i = 1;
        while (i < list.Count) {
            if (comparer.Equals(list[i], current)) {
                list.RemoveAt(i);
                continue;
            }
            current = list[i];
            i++;
        }
    }

    // Removes duplicates from List using a HashSet in 1 iteration
    private static void RemoveDuplicatesNotSorted<T>(this List<T> list, IEqualityComparer<T>? comparer) {
        if (list is { Count: <= 1 }) {
            return;
        }
        var hSet = new HashSet<T>(comparer);
        int i = 0;
        while (i < list.Count) {
            if (hSet.Add(list[i])) {
                i++;
                continue;
            }
            list.RemoveAt(i);
        }
    }

    /// <summary>
    /// Splits the input array into a list of array segments of the specified size.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the array.</typeparam>
    /// <param name="arr">The input array to split.</param>
    /// <param name="sizeOfChunk">The size of each chunk.</param>
    /// <returns>A list of array segments of the specified size.</returns>
    /// <remarks>
    /// This is slightly less efficient than spans but ArraySegment is more flexible since it has less restrictions
    /// </remarks>
    public static List<ArraySegment<T>> ChunkToSegments<T>(this T[] arr, int sizeOfChunk) {
        Debug.Assert(sizeOfChunk > 0, "Size of chunk must be greater than 0");
        var list = new List<ArraySegment<T>>();
        if (arr.Length is 0) {
            return list;
        }
        int i = 0;
        while (i < arr.Length) {
            var remaining = arr.Length - i;
            if (remaining <= sizeOfChunk) {
                list.Add(new ArraySegment<T>(arr, i, remaining));
                break;
            }
            list.Add(new ArraySegment<T>(arr, i, sizeOfChunk));
            i += sizeOfChunk;
        }
        return list;
    }
}