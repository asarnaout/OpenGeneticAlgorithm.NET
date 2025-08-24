using OpenGA.Net.CrossoverStrategies;

namespace OpenGA.Net.OperatorSelectionPolicies;

/// <summary>
/// Abstract base class for operator selection policies that adaptively choose crossover operators.
/// </summary>
/// <typeparam name="T">The type of genes in the chromosome</typeparam>
public abstract class OperatorSelectionPolicy<T>
{
    /// <summary>
    /// Selects a crossover operator based on the policy's selection mechanism.
    /// </summary>
    /// <param name="random">Random number generator</param>
    /// <returns>The selected crossover operator</returns>
    public abstract BaseCrossoverStrategy<T> SelectOperator(Random random);
    
    /// <summary>
    /// Updates the policy with performance feedback from a crossover operation.
    /// </summary>
    /// <param name="crossoverOperator">The crossover operator that was used</param>
    /// <param name="bestParentFitness">Fitness of the best parent before crossover</param>
    /// <param name="bestOffspringFitness">Fitness of the best offspring after crossover</param>
    /// <param name="populationFitnessRange">Current fitness range in the population for normalization</param>
    /// <param name="offspringDiversity">Measure of diversity among the offspring (optional)</param>
    public abstract void UpdateReward(
        BaseCrossoverStrategy<T> crossoverOperator,
        double bestParentFitness,
        double bestOffspringFitness,
        double populationFitnessRange,
        double offspringDiversity = 0.0);
}
