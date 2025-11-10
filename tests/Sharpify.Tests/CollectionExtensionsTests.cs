namespace Sharpify.Tests;

public class CollectionExtensionsTests {
    [Fact]
    public void AsSpan_GivenNonEmptyList_ReturnsCorrectSpan() {
        // Arrange
        var list = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        var span = list.AsSpan();

        // Assert
        Assert.Equal(list.Count, span.Length);
        for (int i = 0; i < list.Count; i++) {
            Assert.Equal(list[i], span[i]);
        }
    }

    [Fact]
    public void AsSpan_GivenEmptyList_ReturnsEmptySpan() {
        // Arrange
        var list = new List<int>();

        // Act
        var span = list.AsSpan();

        // Assert
        Assert.Equal(0, span.Length);
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
        Assert.True(Unsafe.AreSame(ref valueRef, ref valueReal));
    }

    [Fact]
    public void GetValueRefOrNullRef_GivenNonExistingKey_ReturnsRefNull() {
        // Arrange
        var dictionary = new Dictionary<int, string>();
        int key = 1;

        // Act
        ref var valueRef = ref dictionary.GetValueRefOrNullRef(key);

        // Assert
        Assert.True(Unsafe.IsNullRef(ref valueRef));
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
        Assert.Equal("two", valueRef);
        Assert.True(exists);
    }

    [Fact]
    public void GetValueRefOrAddDefault_GivenNonExistingKey_AddsNewEntryWithDefaultValueAndReturnsRefToValue() {
        // Arrange
        var dictionary = new Dictionary<int, string>();
        int key = 1;

        // Act
        ref var valueRef = ref dictionary.GetValueRefOrAddDefault(key, out bool exists);

        // Assert
        #pragma warning disable
        Assert.Equal(default(string), valueRef);
        Assert.False(exists);
        Assert.Contains(new KeyValuePair<int, string>(key, default(string)), dictionary);
        #pragma warning restore
    }

    [Fact]
    public void PureSort_GivenUnsortedIntArray_ReturnsSortedIntArray() {
        // Arrange
        var source = new int[] { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
        var expected = new int[] { 1, 1, 2, 3, 3, 4, 5, 5, 5, 6, 9 };

        // Act
        var result = source.PureSort(Comparer<int>.Default);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PureSort_GivenUnsortedStringArray_ReturnsSortedStringArray() {
        // Arrange
        var source = new string[] { "zoo", "apple", "banana", "cherry", "pear" };
        var expected = new string[] { "apple", "banana", "cherry", "pear", "zoo" };

        // Act
        var result = source.PureSort(StringComparer.InvariantCulture);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PureSort_GivenUnsortedIntList_ReturnsSortedIntList() {
        // Arrange
        var source = new List<int> { 3, 1, 4, 1, 5, 9, 2, 6, 5, 3, 5 };
        var expected = new List<int> { 1, 1, 2, 3, 3, 4, 5, 5, 5, 6, 9 };

        // Act
        var result = source.PureSort(Comparer<int>.Default);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void PureSort_GivenUnsortedStringList_ReturnsSortedStringList() {
        // Arrange
        var source = new List<string> { "zoo", "apple", "banana", "cherry", "pear" };
        var expected = new List<string> { "apple", "banana", "cherry", "pear", "zoo" };

        // Act
        var result = source.PureSort(Comparer<string>.Default);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void RemoveDuplicates_GivenListWithDuplicates_RemovesDuplicates() {
        // Arrange
        var list = new List<int> { 1, 2, 1, 2, 3, 2, 3, 4, 5, 3, 4, 5, 5 };
        var expected = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        list.RemoveDuplicates();

        // Assert
        Assert.Equal(expected, list);
    }

    [Fact]
    public void RemoveDuplicatesWithHashSet_GivenListWithDuplicates_RemovesDuplicatesAndReturnsHashSet() {
        // Arrange
        var list = new List<int> { 1, 2, 1, 2, 3, 2, 3, 4, 5, 3, 4, 5, 5 };
        var expected = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        list.RemoveDuplicates(out var hSet);

        // Assert
        Assert.Equal(expected, list);
        Assert.Equal(expected.Count, hSet.Count);
    }

    [Fact]
    public void RemoveDuplicates_GivenListWithNoDuplicates_DoesNotModifyList() {
        // Arrange
        var list = new List<string> { "banana", "apple", "pear", "cherry"  };
        var expected = new List<string> { "banana", "apple", "pear", "cherry"  };

        // Act
        list.RemoveDuplicates(comparer: StringComparer.InvariantCulture);

        // Assert
        Assert.Equal(expected, list);
    }

    [Fact]
    public void RemoveDuplicates_Sorted_GivenSortedListWithDuplicates_RemovesDuplicates() {
        // Arrange
        var list = new List<int> { 1, 1, 2, 2, 2, 3, 3, 4, 5, 5, 5 };
        var expected = new List<int> { 1, 2, 3, 4, 5 };

        // Act
        list.RemoveDuplicates(isSorted: true);

        // Assert
        Assert.Equal(expected, list);
    }

    [Fact]
    public void RemoveDuplicates_Sorted_GivenSortedListWithNoDuplicates_DoesNotModifyList() {
        // Arrange
        var list = new List<string> { "apple", "banana", "cherry", "pear" };
        var expected = new List<string> { "apple", "banana", "cherry", "pear" };

        // Act
        list.RemoveDuplicates(isSorted: true, comparer: StringComparer.InvariantCulture);

        // Assert
        Assert.Equal(expected, list);
    }

    [Fact]
    public void ChunkToSegments_GivenEmptyArray_ReturnsEmptyList() {
        // Arrange
        var array = new int[0];

        // Act
        var result = array.ChunkToSegments(3);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void ChunkToSegments_GivenArrayWithLengthLessThanSegmentSize_ReturnsSingleSegment() {
        // Arrange
        var array = new int[] { 1, 2, 3 };

        // Act
        var result = array.ChunkToSegments(5);

        // Assert
        Assert.Single(result);
        Assert.Equal(array, result[0]);
    }

    [Fact]
    public void ChunkToSegments_GivenValidArray_ReturnsCorrectNumberOfSegments() {
        // Arrange
        var array = new int[] { 1, 2, 3, 4, 5, 6, 7 };

        // Act
        var result = array.ChunkToSegments(3);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal(array.Length, result.Sum(s => s.Count));
    }
}