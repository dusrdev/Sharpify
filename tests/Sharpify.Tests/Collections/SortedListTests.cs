using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class SortedListTests {
	[Fact]
	public void SortedList_Add() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 4, 5]);

		// Act
		list.Add(6);

		// Assert
		Assert.Equal(list.Count - 1, list.GetIndex(6));

		// Act
		int count = list.Count;
		list.Add(3);

		// Assert
		// Duplicates should be ignored, no change to count
		Assert.Equal(count, list.Count);
	}

	[Fact]
	public void SortedList_AddRange_Span() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 4, 5]);

		// Act
		list.AddRange(new ReadOnlySpan<int>([6, 7, 8]));

		// Assert
		Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], list.Span);
	}

	[Fact]
	public void SortedList_AddRange_IEnumerable() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 4, 5]);

		// Act
		list.AddRange(new List<int>() { 6, 7, 8 });

		// Assert
		Assert.Equal([1, 2, 3, 4, 5, 6, 7, 8], list.Span);
	}

	[Fact]
	public void SortedList_Remove() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 4, 5], null, true);

		// Act
		list.Remove(3);

		// Assert
		Assert.True(list.GetIndex(3) < 0);

		// Act
		for (int i = 0; i < 5; i++) {
			list.Add(6);
		}
		Assert.Equal(5 - 1 + 5, list.Count);
		list.Remove(6);

		// Assert
		Assert.True(list.GetIndex(6) < 0);
	}

	[Fact]
	public void SortedList_GetIndex_Existing() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 4, 5]);

		// Assert
		Assert.Equal(3, list.GetIndex(4));
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 5, 6]);

		// Assert
		Assert.Equal(3, ~list.GetIndex(4));
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_Larger() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 5, 6]);

		// Assert
		Assert.True(~list.GetIndex(7) > list.Count - 1);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_LargerSection() {
		// Arrange
		var list = new SortedList<int>([1, 2, 3, 5, 6]);

		// Act
		var index = list.GetIndex(4);
		ReadOnlySpan<int> section = list.Span.Slice(~index);

		// Assert
		Assert.Equal([5, 6], section);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_LargerSection_Class() {
		// Arrange
		var list = new SortedList<Person>(
			[
				new Person("a", 1),
				new Person("b", 2),
				new Person("c", 3),
				new Person("d", 5),
				new Person("e",6) ]
			);

		// Act + Assert
		var section = list.Span.Slice(~list.GetIndex(new Person("f", 4)));
		Assert.Equal([new Person("d", 5), new Person("e", 6)], section);

		// Act + Assert
		section = list.Span.Slice(list.GetIndex(new Person("f", 3)) + 1);
		Assert.Equal([new Person("d", 5), new Person("e", 6)], section);
	}

	private record Person(string Name, int Age) : IComparable<Person> {
		public int CompareTo(Person? other) {
			if (other is null) {
				return 1;
			}
			return Age.CompareTo(other.Age);
		}
	}
}
