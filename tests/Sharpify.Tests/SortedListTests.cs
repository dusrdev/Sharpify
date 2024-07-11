using Sharpify.Collections;

namespace Sharpify.Tests;

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
}
