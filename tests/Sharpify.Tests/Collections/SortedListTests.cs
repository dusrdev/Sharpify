using Sharpify.Collections;

namespace Sharpify.Tests.Collections;

public class SortedListTests {
	[Fact]
	public void SortedList_Add() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 4, 5 });

		// Act
		list.Add(6);

		// Assert
		list.GetIndex(6).Should().Be(list.Count - 1);

		// Act
		int count = list.Count;
		list.Add(3);

		// Assert
		// Duplicates should be ignored, no change to count
		list.Count.Should().Be(count);
	}

	[Fact]
	public void SortedList_AddRange_Span() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 4, 5 });

		// Act
		list.AddRange(new ReadOnlySpan<int>(new[] { 6, 7, 8 }));

		// Assert
		list.Span.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Should().BeTrue();
	}

	[Fact]
	public void SortedList_AddRange_IEnumerable() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 4, 5 });

		// Act
		list.AddRange(new List<int>() { 6, 7, 8 });

		// Assert
		list.Span.SequenceEqual(new[] { 1, 2, 3, 4, 5, 6, 7, 8 }).Should().BeTrue();
	}

	[Fact]
	public void SortedList_Remove() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 4, 5 }, null, true);

		// Act
		list.Remove(3);

		// Assert
		list.GetIndex(3).Should().BeLessThan(0);

		// Act
		for (int i = 0; i < 5; i++) {
			list.Add(6);
		}
		list.Count.Should().Be(5 - 1 + 5);
		list.Remove(6);

		// Assert
		list.GetIndex(6).Should().BeLessThan(0);
	}

	[Fact]
	public void SortedList_GetIndex_Existing() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 4, 5 });

		// Assert
		list.GetIndex(4).Should().Be(3);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 5, 6 });

		// Assert
		(~list.GetIndex(4)).Should().Be(3);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_Larger() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 5, 6 });

		// Assert
		(~list.GetIndex(7)).Should().BeGreaterThan(list.Count - 1);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_LargerSection() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 5, 6 });

		// Act
		var index = list.GetIndex(4);
		ReadOnlySpan<int> section = list.Span.Slice(~index);

		// Assert
		section.SequenceEqual(new[] { 5, 6 }).Should().BeTrue();
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
		section.SequenceEqual([ new Person("d", 5), new Person("e",6) ]).Should().BeTrue();

		// Act + Assert
		section = list.Span.Slice(list.GetIndex(new Person("f", 3)) + 1);
		section.SequenceEqual([ new Person("d", 5), new Person("e",6) ]).Should().BeTrue();
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
