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
		list.GetIndex(3).Should().Be(-1);

		// Act
		for (int i = 0; i < 5; i++) {
			list.Add(6);
		}
		list.Count.Should().Be(5 - 1 + 5);
		list.Remove(6);

		// Assert
		list.GetIndex(6).Should().Be(-1);
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
		list.GetIndex(4, true).Should().Be(3);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_Larger() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 5, 6 });

		// Assert
		list.GetIndex(7, true).Should().BeGreaterThan(list.Count - 1);
	}

	[Fact]
	public void SortedList_GetIndex_OrIndexOfInsertion_LargerSection() {
		// Arrange
		var list = new SortedList<int>(new[] { 1, 2, 3, 5, 6 });

		// Act
		var index = list.GetIndex(4, true);
		ReadOnlySpan<int> section = list.Span.Slice(index);

		// Assert
		section.SequenceEqual(new[] { 5, 6 }).Should().BeTrue();
	}
}
