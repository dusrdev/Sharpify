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
        list.Sort(comparer);
        return list;
    }

    /// <summary>
    /// Removes duplicates for a sorted list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="comparer"></param>
    /// <remarks>
    /// <para>If you are using a built-in type you can specify the <see cref="Comparer{T}.Default"/></para>
    /// <para>Do not use this method on unsorted lists, it will not work.</para>
    /// <para>For unsorted you can use <see cref="SortAndRemoveDuplicates{T}(List{T}, IComparer{T})"/></para>
    /// </remarks>
    public static void RemoveDuplicatesSorted<T>(this List<T> list, IComparer<T> comparer) {
        if (list is { Count: <= 1 }) {
            return;
        }
        var current = list[0];
        int i = 1;
        while (i < list.Count) {
            if (comparer.Compare(list[i], current) is 0) {
                list.RemoveAt(i);
                continue;
            }
            current = list[i];
            i++;
        }
    }

    /// <summary>
    /// Sorts and removes duplicates for a list
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="list"></param>
    /// <param name="comparer"></param>
    /// <remarks>
    /// If you are using a built-in type you can specify the <see cref="Comparer{T}.Default"/>
    /// </remarks>
    public static void SortAndRemoveDuplicates<T>(this List<T> list, IComparer<T> comparer) {
        list.Sort(comparer);
        list.RemoveDuplicatesSorted(comparer);
    }
}