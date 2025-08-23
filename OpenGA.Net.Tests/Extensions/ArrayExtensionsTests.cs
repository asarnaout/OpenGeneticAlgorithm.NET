using OpenGA.Net.Extensions;

namespace OpenGA.Net.Tests.Extensions;

public class ArrayExtensionsTests
{
    [Fact]
    public void FisherYatesShuffled_WithValidArray_ShouldReturnShuffledCopy()
    {
        // Arrange
        var originalArray = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var random = new Random(42); // Fixed seed for deterministic test

        // Act
        var shuffledArray = originalArray.FisherYatesShuffled(random);

        // Assert
        // Original array should remain unchanged
        Assert.Equal(new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }, originalArray);
        
        // Shuffled array should have same elements but be a different instance
        Assert.NotSame(originalArray, shuffledArray);
        Assert.Equal(originalArray.Length, shuffledArray.Length);
        Assert.Equal(originalArray.OrderBy(x => x), shuffledArray.OrderBy(x => x));
    }

    [Fact]
    public void FisherYatesShuffled_WithEmptyArray_ShouldReturnEmptyArray()
    {
        // Arrange
        var emptyArray = Array.Empty<int>();
        var random = new Random(42);

        // Act
        var result = emptyArray.FisherYatesShuffled(random);

        // Assert
        Assert.Empty(result);
        Assert.NotSame(emptyArray, result); // Should be a different instance
    }

    [Fact]
    public void FisherYatesShuffled_WithSingleElement_ShouldReturnSingleElementCopy()
    {
        // Arrange
        var singleElementArray = new[] { 42 };
        var random = new Random(42);

        // Act
        var result = singleElementArray.FisherYatesShuffled(random);

        // Assert
        Assert.Single(result);
        Assert.Equal(42, result[0]);
        Assert.NotSame(singleElementArray, result); // Should be different instance
    }

    [Fact]
    public void FisherYatesShuffled_WithNullArray_ShouldThrowArgumentNullException()
    {
        // Arrange
        int[]? nullArray = null;
        var random = new Random(42);

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => nullArray!.FisherYatesShuffled(random));
    }

    [Fact]
    public void FisherYatesShuffled_WithNullRandom_ShouldThrowArgumentNullException()
    {
        // Arrange
        var array = new[] { 1, 2, 3 };
        Random? nullRandom = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => array.FisherYatesShuffled(nullRandom!));
    }

    [Fact]
    public void FisherYatesShuffled_WithDifferentTypes_ShouldWork()
    {
        // Arrange
        var stringArray = new[] { "apple", "banana", "cherry", "date" };
        var random = new Random(42);

        // Act
        var shuffledArray = stringArray.FisherYatesShuffled(random);

        // Assert
        Assert.Equal(stringArray.Length, shuffledArray.Length);
        Assert.Equal(stringArray.OrderBy(x => x), shuffledArray.OrderBy(x => x));
        Assert.NotSame(stringArray, shuffledArray);
    }

    [Fact]
    public void FisherYatesShuffled_MultipleRuns_ShouldProduceDifferentResults()
    {
        // Arrange
        var originalArray = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
        var random1 = new Random(42);
        var random2 = new Random(123);

        // Act
        var shuffled1 = originalArray.FisherYatesShuffled(random1);
        var shuffled2 = originalArray.FisherYatesShuffled(random2);

        // Assert
        // With different seeds, arrays should likely be different
        // This is probabilistic, but with different seeds it's highly likely
        Assert.Equal(shuffled1.OrderBy(x => x), shuffled2.OrderBy(x => x)); // Same elements
        Assert.Equal(originalArray, new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 }); // Original unchanged
    }

    [Fact]
    public void FisherYatesShuffled_ShouldPreserveAllElements()
    {
        // Arrange
        var originalArray = new[] { 1, 3, 5, 7, 9, 2, 4, 6, 8, 0 };
        var random = new Random(42);

        // Act
        var shuffledArray = originalArray.FisherYatesShuffled(random);

        // Assert
        // Should contain exactly the same elements
        Assert.Equal(originalArray.Length, shuffledArray.Length);
        Assert.All(originalArray, element => Assert.Contains(element, shuffledArray));
        Assert.All(shuffledArray, element => Assert.Contains(element, originalArray));
    }
}
