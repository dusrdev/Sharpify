namespace Sharpify.Tests;

public class CollectionExtensionsTests {
    [Fact]
    public void AsSpan_GivenNonEmptyList_ReturnsCorrectSpan() {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var span = list.AsSpan();

        // Assert
        span.Length.Should().Be(list.Count);
        for (int i = 0; i < list.Count; i++) {
            span[i].Should().Be(list[i]);
        }
    }

    [Fact]
    public void AsSpan_GivenEmptyList_ReturnsEmptySpan() {
        // Arrange
        var list = new List<int>();

        // Act
        var span = list.AsSpan();

        // Assert
        span.Length.Should().Be(0);
    }

    [Fact]
    public void GetValueRefOrNullRef_GivenExistingKey_ReturnsRefToValue() {
        // Arrange
        var dictionary = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" },
        };
        int key = 2;

        // Act
        ref var valueRef = ref dictionary.GetValueRefOrNullRef(key);
        ref var valueReal = ref CollectionsMarshal.GetValueRefOrNullRef(dictionary, key);

        // Assert
        Unsafe.AreSame(ref valueRef, ref valueReal).Should().BeTrue();
    }

    [Fact]
    public void GetValueRefOrNullRef_GivenNonExistingKey_ReturnsRefNull() {
        // Arrange
        var dictionary = new Dictionary<int, string>();
        int key = 1;

        // Act
        ref var valueRef = ref dictionary.GetValueRefOrNullRef(key);

        // Assert
        Unsafe.IsNullRef(ref valueRef).Should().BeTrue();
    }

    [Fact]
    public void GetValueRefOrAddDefault_GivenExistingKey_ReturnsRefToValueAndDoesNotAddNewEntry() {
        // Arrange
        var dictionary = new Dictionary<int, string>
        {
            { 1, "one" },
            { 2, "two" },
            { 3, "three" },
        };
        int key = 2;

        // Act
        ref var valueRef = ref dictionary.GetValueRefOrAddDefault(key, out bool exists);

        // Assert
        valueRef.Should().BeEquivalentTo("two");
        exists.Should().BeTrue();
    }

    [Fact]
    public void GetValueRefOrAddDefault_GivenNonExistingKey_AddsNewEntryWithDefaultValueAndReturnsRefToValue() {
        // Arrange
        var dictionary = new Dictionary<int, string>();
        int key = 1;

        // Act
        ref var valueRef = ref dictionary.GetValueRefOrAddDefault(key, out bool exists);

        // Assert
        valueRef.Should().Be(default(string));
        exists.Should().BeTrue();
        dictionary.Should().ContainKey(key).And.ContainValue(default(string));
    }

    [Fact]
    public void PureSort_GivenUnsortedIntArray_ReturnsSortedIntArray() {
        // Arrange
        var source = new int[] { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
        var expected = new int[] { 1, 1, 2, 3, 3, 4, 5, 5, 5, 6, 9 };

        // Act
        var result = source.PureSort(Comparer<int>.Default);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void PureSort_GivenUnsortedStringArray_ReturnsSortedStringArray() {
        // Arrange
        var source = new string[] { "zoo", "apple", "banana", "cherry", "pear" };
        var expected = new string[] { "apple", "banana", "cherry", "pear", "zoo" };

        // Act
        var result = source.PureSort(StringComparer.InvariantCulture);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void PureSort_GivenUnsortedIntList_ReturnsSortedIntList() {
        // Arrange
        var source = new List<int> { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
        var expected = new List<int> { 1, 1, 2, 3, 3, 4, 5, 5, 5, 6, 9 };

        // Act
        var result = source.PureSort(Comparer<int>.Default);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void PureSort_GivenUnsortedStringList_ReturnsSortedStringList() {
        // Arrange
        var source = new List<string> { "zoo", "apple", "banana", "cherry", "pear" };
        var expected = new List<string> { "apple", "banana", "cherry", "pear", "zoo" };

        // Act
        var result = source.PureSort(Comparer<string>.Default);

        // Assert
        result.Should().Equal(expected);
    }

    [Fact]
    public void RemoveDuplicatesSorted_GivenSortedListWithDuplicates_RemovesDuplicates() {
        // Arrange
        var list = new List<int> { 1, 1, 2, 2, 2, 3, 3, 4, 5, 5, 5 };
        var expected = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        list.RemoveDuplicatesSorted(Comparer<int>.Default);

        // Assert
        list.Should().Equal(expected);
    }

    [Fact]
    public void RemoveDuplicatesSorted_GivenSortedListWithNoDuplicates_DoesNotModifyList() {
        // Arrange
        var list = new List<string> { "apple", "banana", "cherry", "pear" };
        var expected = new List<string> { "apple", "banana", "cherry", "pear" };

        // Act
        list.RemoveDuplicatesSorted(StringComparer.InvariantCulture);

        // Assert
        list.Should().Equal(expected);
    }
}