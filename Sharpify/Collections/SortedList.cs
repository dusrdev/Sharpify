using System.Collections;

namespace Sharpify.Collections;

/// <summary>
/// Represents a sorted list of elements that can be accessed by index. Provides methods to search, sort, and manipulate lists.
/// </summary>
/// <typeparam name="T">The type of elements in the list.</typeparam>
public class SortedList<T> : IReadOnlyList<T> {
	/// <summary>
	/// The underlying list used for storing elements in the SortedList.
	/// </summary>
	protected readonly List<T> _list;
	/// <summary>
	/// The comparer used to compare elements in the sorted list.
	/// </summary>
	protected readonly IComparer<T> _comparer;
	/// <summary>
	/// Gets a value indicating whether the SortedList allows duplicate elements.
	/// </summary>
	protected readonly bool _allowDuplicates;

	/// <summary>
	/// Initializes a new instance of the <see cref="SortedList{T}"/> class that is empty, has the default initial capacity, and uses the default comparer for the element type.
	/// </summary>
	public SortedList() : this(null, Comparer<T>.Default, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SortedList{T}"/> class that contains elements copied from the specified collection
	/// </summary>
	public SortedList(IEnumerable<T>? collection) : this(collection, Comparer<T>.Default, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SortedList{T}"/> class that contains elements copied from the specified collection and uses the specified <see cref="Comparer{T}"/> implementation to compare elements.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new <see cref="SortedList{T}"/>.</param>
	/// <param name="comparer">The <see cref="Comparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default comparer <see cref="Comparer{T}.Default"/>.</param>
	public SortedList(IEnumerable<T>? collection, Comparer<T>? comparer) : this(collection, comparer, false) { }

	/// <summary>
	/// Initializes a new instance of the <see cref="SortedList{T}"/> class that contains elements copied from the specified collection and uses the specified comparer and duplicate element allowance.
	/// </summary>
	/// <param name="collection">The collection whose elements are copied to the new list.</param>
	/// <param name="comparer">The <see cref="Comparer{T}"/> implementation to use when comparing elements, or <see langword="null"/> to use the default comparer <see cref="Comparer{T}.Default"/>.</param>
	/// <param name="allowDuplicates">A value indicating whether the list allows duplicate elements.</param>
	public SortedList(IEnumerable<T>? collection, Comparer<T>? comparer, bool allowDuplicates) {
		_list = collection is null ? new() : new(collection);
		_comparer = comparer ?? Comparer<T>.Default;
		_allowDuplicates = allowDuplicates;
		if (Count < 0) {
			return;
		}
		_list.Sort(_comparer);
		if (_allowDuplicates) {
			return;
		}
		_list.RemoveDuplicatesFromSorted(_comparer);
	}

	/// <summary>
	/// Gets the number of elements contained in the <see cref="SortedList{T}"/>.
	/// </summary>
	public int Count => _list.Count;

	/// <summary>
	/// Gets the element at the given index
	/// </summary>
	/// <param name="index"></param>
	public T this[int index] => _list[index];

	/// <summary>
	/// Gets the index of the specified item in the sorted list.
	/// </summary>
	/// <param name="item">The item to search for.</param>
	/// <returns>The zero-based index of the item in the sorted list, or -1 if not found.</returns>
	public int GetIndex(T item) {
		var index = _list.BinarySearch(item, _comparer);
		return index >= 0 ? index : -1;
	}

	/// <summary>
	/// Adds an item to the sorted list.
	/// </summary>
	/// <param name="item">The item to add.</param>
	public void Add(T item) {
		var index = _list.BinarySearch(item);
		if (index >= 0 && !_allowDuplicates) {
			return;
		}
		if (index < 0) {
			index = ~index;
		}
		_list.Insert(index, item);
	}

	/// <summary>
	/// Removes the first occurrence of a specific object from the <see cref="SortedList{T}"/>.
	/// </summary>
	/// <param name="item">The object to remove from the <see cref="SortedList{T}"/>.</param>
	/// <remarks>
	/// If <see cref="_allowDuplicates"/> is false, the method removes only the first occurrence of the specified object.
	/// If <see cref="_allowDuplicates"/> is true, the method removes all occurrences of the specified object.
	/// </remarks>
	public void Remove(T item) {
		if (Count is 0) {
			return;
		}
		var index = _list.BinarySearch(item, _comparer);
		if (index < 0) {
			return;
		}
		if (!_allowDuplicates) {
			_list.RemoveAt(index);
			return;
		}
		// AllowDuplicated is true, remove range
		int count = 0;
		while (index > 0 && _comparer.Compare(_list[index], item) is 0) {
			index--;
		}
		index++;
		while (index + count < Count && _comparer.Compare(_list[index + count], item) is 0) {
			count++;
		}
		_list.RemoveRange(index, count);
	}

	/// <summary>
	/// Removes the element at the specified index of the <see cref="SortedList{T}"/>.
	/// </summary>
	/// <param name="index">The zero-based index of the element to remove.</param>
	public void RemoveAt(int index) => _list.RemoveAt(index);

	/// <summary>
	/// Gets a <see cref="Span{T}"/> that represents the entire underlying list.
	/// </summary>
	public Span<T> Span => _list.AsSpan();

	/// <summary>
	/// Removes all elements from the SortedList.
	/// </summary>
	public void Clear() => _list.Clear();

	/// <summary>
	/// Returns the inner list of the sorted list.
	/// </summary>
	/// <remarks>
	/// Use this to pass the sorted list to methods that expect a list.
	/// </remarks>
	public static implicit operator List<T>(SortedList<T> sortedList) => sortedList._list;

	/// <summary>
	/// Returns an enumerator that iterates through the sorted list.
	/// </summary>
	/// <returns>An enumerator for the sorted list.</returns>
	public IEnumerator<T> GetEnumerator() => _list.GetEnumerator();

	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
