namespace OpenGA.Net.Extensions;

/// <summary>
/// Extension methods for arrays to provide common utility operations.
/// </summary>
public static class ArrayExtensions
{
    /// <summary>
    /// Creates a shuffled copy of the array using the Fisher-Yates shuffle algorithm.
    /// The original array remains unchanged.
    /// </summary>
    /// <typeparam name="T">The type of elements in the array</typeparam>
    /// <param name="array">The array to shuffle</param>
    /// <param name="random">Random number generator for the shuffle operation</param>
    /// <returns>A new array containing the same elements in shuffled order</returns>
    /// <exception cref="ArgumentNullException">Thrown when array or random is null</exception>
    /// <remarks>
    /// The Fisher-Yates shuffle algorithm has O(n) time complexity and guarantees that each
    /// possible permutation of the array has equal probability of occurring.
    /// </remarks>
    public static T[] FisherYatesShuffled<T>(this T[] array, Random random)
    {
        ArgumentNullException.ThrowIfNull(array, nameof(array));
        ArgumentNullException.ThrowIfNull(random, nameof(random));

        var shuffledArray = new T[array.Length];
        Array.Copy(array, shuffledArray, array.Length);
        
        // Fisher-Yates shuffle algorithm
        for (int i = shuffledArray.Length - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffledArray[i], shuffledArray[j]) = (shuffledArray[j], shuffledArray[i]);
        }
        
        return shuffledArray;
    }
}
