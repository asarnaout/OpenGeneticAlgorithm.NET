namespace OpenGA.Net.ReplacementStrategies;

public class ReplacementStrategyConfiguration<T>
{
    internal BaseReplacementStrategy<T> ReplacementStrategy = default!;

    /// <summary>
    /// Apply random elimination replacement strategy. Eliminates chromosomes randomly from the population 
    /// to make room for offspring, ensuring population size is maintained.
    /// Each chromosome has an equal chance of being eliminated.
    /// </summary>
    public BaseReplacementStrategy<T> ApplyRandomEliminationReplacementStrategy()
    {
        var result = new RandomEliminationReplacementStrategy<T>();
        ReplacementStrategy = result;
        return result;
    }

    /// <summary>
    /// Apply generational replacement strategy. Completely replaces the entire parent population with offspring.
    /// In this strategy, no parent chromosomes survive to the next generation - the entire population
    /// is renewed with the offspring generation.
    /// </summary>
    public BaseReplacementStrategy<T> ApplyGenerationalReplacementStrategy()
    {
        var result = new GenerationalReplacementStrategy<T>();
        ReplacementStrategy = result;
        return result;
    }

    /// <summary>
    /// Apply elitist replacement strategy. Protects the top-performing chromosomes (elites) from elimination
    /// based on their fitness values, while allowing the remaining population to be replaced with offspring.
    /// This ensures that the best solutions are preserved across generations.
    /// </summary>
    /// <param name="elitePercentage">
    /// The percentage of the population to protect as elites (0.0 to 1.0).
    /// Default is 0.1 (10%). Must be between 0.0 and 1.0.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when elitePercentage is not between 0.0 and 1.0.
    /// </exception>
    public BaseReplacementStrategy<T> ApplyElitistReplacementStrategy(float elitePercentage = 0.1f)
    {
        if (elitePercentage < 0.0f || elitePercentage > 1.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elitePercentage), 
                elitePercentage, 
                "Elite percentage must be between 0.0 and 1.0.");
        }

        var result = new ElitistReplacementStrategy<T>(elitePercentage);
        ReplacementStrategy = result;
        return result;
    }

    /// <summary>
    /// Apply tournament-based replacement strategy. Eliminates chromosomes through competitive tournaments
    /// where the least fit individuals are more likely to be eliminated.
    /// </summary>
    /// <param name="tournamentSize">
    /// The number of chromosomes that participate in each tournament. Must be at least 3.
    /// Larger tournaments increase selection pressure towards eliminating less fit chromosomes.
    /// </param>
    /// <param name="stochasticTournament">
    /// If true, uses weighted random selection within tournaments based on inverse fitness.
    /// If false, always eliminates the least fit chromosome in each tournament.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when tournamentSize is less than 3.
    /// </exception>
    public BaseReplacementStrategy<T> ApplyTournamentReplacementStrategy(int tournamentSize = 3, bool stochasticTournament = true)
    {
        if (tournamentSize < 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tournamentSize), 
                tournamentSize, 
                "Tournament size must be at least 3.");
        }

        var result = new TournamentReplacementStrategy<T>(tournamentSize, stochasticTournament);
        ReplacementStrategy = result;
        return result;
    }

    /// <summary>
    /// Apply a custom replacement strategy. Requires an instance of a subclass of <see cref="BaseReplacementStrategy<T>">BaseReplacementStrategy<T></see>
    /// to dictate how chromosomes are eliminated from the population to make room for offspring.
    /// </summary>
    /// <param name="replacementStrategy">The custom replacement strategy to apply</param>
    public BaseReplacementStrategy<T> ApplyCustomReplacementStrategy(BaseReplacementStrategy<T> replacementStrategy)
    {
        ArgumentNullException.ThrowIfNull(replacementStrategy, nameof(replacementStrategy));
        ReplacementStrategy = replacementStrategy;
        return replacementStrategy;
    }
}
