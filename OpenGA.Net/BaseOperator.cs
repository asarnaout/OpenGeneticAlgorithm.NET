using System.Runtime.CompilerServices;

namespace OpenGA.Net;

/// <summary>
/// Abstract base class for all genetic algorithm operators.
/// 
/// This class serves as the foundation for all genetic operators including crossover strategies,
/// mutation operators, selection strategies, and any other genetic algorithm components that
/// can be selected and applied dynamically during the evolution process.
/// 
/// Operators derived from this class can be used with operator selection policies such as
/// Adaptive Pursuit to dynamically choose the most effective operator based on performance feedback.
/// </summary>
public abstract class BaseOperator : IEquatable<BaseOperator>
{
    internal float CustomWeight { get; set; }

    /// <summary>
    /// Sets a custom weight for this operator when used in multi-operator scenarios.
    /// 
    /// This method is primarily useful when you have multiple operators of the same type
    /// (e.g., multiple crossover strategies, multiple mutation operators) and want to
    /// influence their selection probability in operator selection policies such as
    /// CustomWeightPolicy or weighted random selection mechanisms.
    /// 
    /// Note: This weight only has meaning when multiple operators are available for selection.
    /// If only a single operator is used, the weight value has no effect on the algorithm's behavior.
    /// </summary>
    /// <param name="weight">
    /// The custom weight to assign to this operator. Must be non-negative.
    /// Higher weights increase the probability of this operator being selected
    /// when multiple operators are available.
    /// </param>
    /// <returns>
    /// The current operator instance to allow for method chaining.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when the weight is negative.
    /// </exception>
    /// <example>
    /// <code>
    /// // Example with multiple crossover strategies
    /// var onePointCrossover = new OnePointCrossoverStrategy().WithCustomWeight(0.3f);
    /// var uniformCrossover = new UniformCrossoverStrategy().WithCustomWeight(0.7f);
    /// 
    /// // The uniform crossover will be selected more frequently due to higher weight
    /// var strategies = new[] { onePointCrossover, uniformCrossover };
    /// </code>
    /// </example>
    internal BaseOperator WithCustomWeight(float weight)
    {
        if (weight < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(weight), "Custom weight must be non-negative.");
        }

        CustomWeight = weight;
        return this;
    }

    /// <summary>
    /// Determines whether the specified BaseOperator is equal to the current BaseOperator.
    /// Uses reference equality by default.
    /// </summary>
    /// <param name="other">The BaseOperator to compare with the current BaseOperator.</param>
    /// <returns>true if the specified BaseOperator is equal to the current BaseOperator; otherwise, false.</returns>
    public virtual bool Equals(BaseOperator? other)
    {
        return ReferenceEquals(this, other);
    }

    /// <summary>
    /// Determines whether the specified object is equal to the current BaseOperator.
    /// </summary>
    /// <param name="obj">The object to compare with the current BaseOperator.</param>
    /// <returns>true if the specified object is equal to the current BaseOperator; otherwise, false.</returns>
    public override bool Equals(object? obj)
    {
        return Equals(obj as BaseOperator);
    }

    /// <summary>
    /// Returns the hash code for this BaseOperator.
    /// Uses reference-based hash code by default.
    /// </summary>
    /// <returns>A hash code for the current BaseOperator.</returns>
    public override int GetHashCode()
    {
        return RuntimeHelpers.GetHashCode(this);
    }
}
