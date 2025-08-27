namespace OpenGA.Net.ReplacementStrategies;

public class ReplacementStrategyConfiguration<T>
{
    internal IList<BaseReplacementStrategy<T>> ReplacementStrategies = [];

    /// <summary>
    /// Apply random elimination replacement strategy. Eliminates chromosomes randomly from the population 
    /// to make room for offspring, ensuring population size is maintained.
    /// Each chromosome has an equal chance of being eliminated.
    /// </summary>
    public BaseReplacementStrategy<T> Random()
    {
        var result = new RandomEliminationReplacementStrategy<T>();
        ReplacementStrategies.Add(result);
        return result;
    }

    /// <summary>
    /// Apply generational replacement strategy. Completely replaces the entire parent population with offspring.
    /// In this strategy, no parent chromosomes survive to the next generation - the entire population
    /// is renewed with the offspring generation.
    /// </summary>
    public BaseReplacementStrategy<T> Generational()
    {
        var result = new GenerationalReplacementStrategy<T>();
        ReplacementStrategies.Add(result);
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
    public BaseReplacementStrategy<T> Elitist(float elitePercentage = 0.1f)
    {
        if (elitePercentage < 0.0f || elitePercentage > 1.0f)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elitePercentage), 
                elitePercentage, 
                "Elite percentage must be between 0.0 and 1.0.");
        }

        var result = new ElitistReplacementStrategy<T>(elitePercentage);
        ReplacementStrategies.Add(result);
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
    public BaseReplacementStrategy<T> Tournament(int tournamentSize = 3, bool stochasticTournament = true)
    {
        if (tournamentSize < 3)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tournamentSize), 
                tournamentSize, 
                "Tournament size must be at least 3.");
        }

        var result = new TournamentReplacementStrategy<T>(tournamentSize, stochasticTournament);
        ReplacementStrategies.Add(result);
        return result;
    }

    /// <summary>
    /// Apply age-based replacement strategy. Eliminates chromosomes based on their age using a weighted
    /// roulette wheel where older chromosomes have higher probability of being eliminated.
    /// This encourages population turnover while maintaining some genetic diversity.
    /// </summary>
    public BaseReplacementStrategy<T> AgeBased()
    {
        var result = new AgeBasedReplacementStrategy<T>();
        ReplacementStrategies.Add(result);
        return result;
    }

    /// <summary>
    /// Apply a custom replacement strategy. Requires an instance of a subclass of <see cref="BaseReplacementStrategy<T>">BaseReplacementStrategy<T></see>
    /// to dictate how chromosomes are eliminated from the population to make room for offspring.
    /// </summary>
    /// <param name="replacementStrategy">The custom replacement strategy to apply</param>
    public BaseReplacementStrategy<T> Custom(BaseReplacementStrategy<T> replacementStrategy)
    {
        ArgumentNullException.ThrowIfNull(replacementStrategy, nameof(replacementStrategy));
        ReplacementStrategies.Add(replacementStrategy);
        return replacementStrategy;
    }

    /// <summary>
    /// Apply Boltzmann replacement strategy that uses temperature-based elimination probabilities with exponential decay.
    /// This strategy applies the Boltzmann distribution to control elimination pressure through a temperature parameter
    /// that starts at the specified initial value and decays exponentially over epochs: T(t) = T₀ × e^(-α×t).
    /// Higher temperature leads to more uniform elimination (exploration), while lower temperature leads to more fitness-based elimination (exploitation).
    /// Uses inverse fitness weighting where chromosomes with lower fitness have higher probability of elimination.
    /// </summary>
    /// <param name="temperatureDecayRate">The exponential decay rate per epoch. Higher values (e.g., 0.1) result in faster cooling, 
    /// lower values (e.g., 0.01) result in slower cooling. Must be greater than or equal to 0. Defaults to 0.05.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public BaseReplacementStrategy<T> Boltzmann(double temperatureDecayRate = 0.05, double initialTemperature = 1.0)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }
        
        var result = new BoltzmannReplacementStrategy<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: true);
        ReplacementStrategies.Add(result);
        return result;
    }

    /// <summary>
    /// Apply Boltzmann replacement strategy that uses temperature-based elimination probabilities with linear decay.
    /// This strategy applies the Boltzmann distribution to control elimination pressure through a temperature parameter
    /// that starts at the specified initial value and decays linearly over epochs: T(t) = T₀ - α×t.
    /// Higher temperature leads to more uniform elimination (exploration), while lower temperature leads to more fitness-based elimination (exploitation).
    /// Uses inverse fitness weighting where chromosomes with lower fitness have higher probability of elimination.
    /// </summary>
    /// <param name="temperatureDecayRate">The linear decay rate per epoch (amount subtracted from temperature each epoch). 
    /// Higher values result in faster cooling. Must be greater than or equal to 0. Defaults to 0.01.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public BaseReplacementStrategy<T> BoltzmannWithLinearDecay(double temperatureDecayRate = 0.01, double initialTemperature = 1.0)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }
        
        var result = new BoltzmannReplacementStrategy<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: false);
        ReplacementStrategies.Add(result);
        return result;
    }
}
