using OpenGA.Net.Exceptions;

namespace OpenGA.Net.ParentSelectors;

public class ParentSelectorConfiguration<T>
{
    internal BaseParentSelector<T> ParentSelector = default!;

    /// <summary>
    /// Parents are chosen at random regardless of their fitness.
    /// </summary>
    public BaseParentSelector<T> Random()
    {
        var result = new RandomParentSelector<T>();
        ParentSelector = result;
        return result;
    }

    /// <summary>
    /// The likelihood of an individual chromosome being chosen for mating is proportional to its fitness.
    /// </summary>
    public BaseParentSelector<T> RouletteWheel()
    {
        var result = new FitnessWeightedRouletteWheelParentSelector<T>();
        ParentSelector = result;
        return result;
    }

    /// <summary>
    /// Each iteration, n-individuals are chosen at random to form a tournament and out of this group, 2 individuals are chosen for mating.
    /// The number of tournaments held per iteration is stochastic and depends on the size of the population at each iteration.
    /// </summary>
    /// <param name="stochasticTournament">Defaults to true. If set to true, then the 2 individuals chosen for mating in each 
    /// tournament are the fittest 2 individuals in the tournament, otherwise a roulette wheel is spun to choose the two winners 
    /// out of the n-individuals, where the probability of winning is proportional to each individual's fitness.</param>
    public BaseParentSelector<T> Tournament(bool stochasticTournament = true)
    {
        var result = new TournamentParentSelector<T>(stochasticTournament);
        ParentSelector = result;
        return result;
    }

    /// <summary>
    /// Apply a custom strategy for choosing mating parents. Requires an instance of a subclass of <see cref="BaseParentSelector<T>">BaseParentSelector<T></see>
    /// to dictate which individuals will be chosen to take part in the crossover process.
    /// </summary>
    public BaseParentSelector<T> Custom(BaseParentSelector<T> parentSelector)
    {
        ArgumentNullException.ThrowIfNull(parentSelector, nameof(parentSelector));
        ParentSelector = parentSelector;
        return parentSelector;
    }

    /// <summary>
    /// Similar to the traditional fitness-weighted roulette wheel selection mechanism, however, Rank Selection
    /// aims to blunt any disproportionate advantage in fitness a chromosome has which will almost always guarantee
    /// its selection over the mid/long term.
    /// 
    /// With Rank Selection, each chromosome's fitness is used to assign it a rank and the rank is used (instead of the absolute
    /// fitness value) to determine the chromosome's advantage in the rouletee wheel. This guarantees that chromosomes with a
    /// disproportionate advantage in fitness will have a (relatively) harder time (compared to the traditional fitness-weighted roulette wheel) 
    /// dominating the selection mechanism.
    /// </summary>
    public BaseParentSelector<T> Rank()
    {
        var result = new RankSelectionParentSelector<T>();
        ParentSelector = result;
        return result;
    }

    /// <summary>
    /// Boltzmann Selection parent selector that uses temperature-based selection probabilities with exponential decay.
    /// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter
    /// that starts at the specified initial value and decays exponentially over epochs: T(t) = T₀ × e^(-α×t).
    /// Higher temperature leads to more uniform selection (exploration), while lower temperature leads to more elitist selection (exploitation).
    /// Exponential decay provides smooth cooling and never reaches absolute zero, maintaining some exploration throughout the run.
    /// </summary>
    /// <param name="temperatureDecayRate">The exponential decay rate per epoch. Higher values (e.g., 0.1) result in faster cooling, 
    /// lower values (e.g., 0.01) result in slower cooling. Must be greater than or equal to 0. Defaults to 0.05.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public BaseParentSelector<T> Boltzmann(double temperatureDecayRate = 0.05, double initialTemperature = 1.0)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }
        
        var result = new BoltzmannParentSelector<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: true);
        ParentSelector = result;
        return result;
    }

    /// <summary>
    /// Boltzmann Selection parent selector that uses temperature-based selection probabilities with linear decay.
    /// This strategy applies the Boltzmann distribution to control selection pressure through a temperature parameter
    /// that starts at the specified initial value and decays linearly over epochs: T(t) = T₀ - α×t.
    /// Higher temperature leads to more uniform selection (exploration), while lower temperature leads to more elitist selection (exploitation).
    /// Linear decay provides predictable cooling and can reach zero temperature for pure exploitation in later epochs.
    /// </summary>
    /// <param name="temperatureDecayRate">The linear decay rate per epoch (amount subtracted from temperature each epoch). 
    /// Higher values result in faster cooling. Must be greater than or equal to 0. Defaults to 0.01.</param>
    /// <param name="initialTemperature">The starting temperature value. Higher values promote more exploration initially.
    /// Must be greater than 0. Defaults to 1.0.</param>
    /// <exception cref="ArgumentException">Thrown when temperatureDecayRate is less than 0 or initialTemperature is less than or equal to 0.</exception>
    public BaseParentSelector<T> BoltzmannWithLinearDecay(double temperatureDecayRate = 0.01, double initialTemperature = 1.0)
    {
        if (temperatureDecayRate < 0)
        {
            throw new ArgumentException("Temperature decay rate must be greater than or equal to 0.", nameof(temperatureDecayRate));
        }
        
        if (initialTemperature <= 0)
        {
            throw new ArgumentException("Initial temperature must be greater than 0.", nameof(initialTemperature));
        }
        
        var result = new BoltzmannParentSelector<T>(temperatureDecayRate, initialTemperature, useExponentialDecay: false);
        ParentSelector = result;
        return result;
    }

    /// <summary>
    /// The fittest individual chromosomes are guaranteed to participate in the mating process in the current epoch/generation. Non elites may participate as well.
    /// </summary>
    /// <param name="proportionOfElitesInPopulation">The proportion of elites in the population. Example, if the rate is 0.2 and the population size is 100, then we have 20 elites who are guaranteed to take part in the mating process.</param>
    /// <param name="proportionOfNonElitesAllowedToMate">The proportion of non-elites allowed to take part in the mating process. Non elites are chosen randomly regardless of fitness.</param>
    /// <param name="allowMatingElitesWithNonElites">Defaults to true. Setting this value to false would restrict couples made up of an elite and non-elite members</param>
    public BaseParentSelector<T> Elitist(float proportionOfElitesInPopulation = 0.1f, float proportionOfNonElitesAllowedToMate = 0.01f, bool allowMatingElitesWithNonElites = true)
    {
        if (proportionOfElitesInPopulation <= 0 || proportionOfElitesInPopulation > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfElitesInPopulation), "Value must be greater than 0 and less than or equal to 1.");
        }

        if (proportionOfNonElitesAllowedToMate < 0 || proportionOfNonElitesAllowedToMate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(proportionOfNonElitesAllowedToMate), "Value must be between 0 and 1.");
        }

        var result = new ElitistParentSelector<T>(allowMatingElitesWithNonElites, proportionOfElitesInPopulation, proportionOfNonElitesAllowedToMate);
        ParentSelector = result;
        return result;
    }
}
