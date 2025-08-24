namespace OpenGA.Net.Extensions;

/// <summary>
/// Mathematical extension methods for common statistical calculations.
/// </summary>
public static class MathExtensions
{
    /// <summary>
    /// Calculates the standard deviation of a collection of numeric values.
    /// Uses the sample standard deviation formula (n-1 denominator).
    /// </summary>
    /// <param name="values">The collection of values to calculate standard deviation for</param>
    /// <returns>The standard deviation of the values</returns>
    /// <exception cref="ArgumentNullException">Thrown when values is null</exception>
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        ArgumentNullException.ThrowIfNull(values);
        
        var valueArray = values.ToArray();
        
        if (valueArray.Length <= 1)
        {
            return 0.0;
        }
        
        var mean = valueArray.Average();
        var squaredDifferences = valueArray.Select(v => Math.Pow(v - mean, 2));
        var variance = squaredDifferences.Average();
        
        return Math.Sqrt(variance);
    }
}
