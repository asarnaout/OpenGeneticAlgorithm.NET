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
