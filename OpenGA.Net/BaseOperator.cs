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
public abstract class BaseOperator
{
}
