using OpenGA.Net.Extensions;

namespace OpenGA.Net.ReproductionSelectors;

/// <summary>
/// A tournament reproduction selector that chooses mating pairs through tournament-based selection.
/// This strategy creates small tournaments from random subsets of the population and selects
/// winners based on fitness, providing a balance between selection pressure and genetic diversity.
/// </summary>
/// <typeparam name="T">The type of the gene data contained in chromosomes</typeparam>
/// <remarks>
/// Tournament selection works by:
/// 1. Creating tournaments of random individuals (5-20% of population size)
/// 2. Selecting winners from each tournament based on fitness
/// 3. Either deterministically choosing the fittest (non-stochastic) or using fitness-weighted selection (stochastic)
/// 
/// Advantages:
/// - Adjustable selection pressure through tournament size
/// - Good balance between exploration and exploitation
/// - Computationally efficient compared to full population ranking
/// - Naturally handles negative fitness values
/// </remarks>
public class TournamentReproductionSelector<T>(bool stochasticTournament) : BaseReproductionSelector<T>
{
    /// <summary>
    /// Gets a value indicating whether tournaments use stochastic (fitness-weighted) or deterministic (best-wins) selection.
    /// </summary>
    /// <value>
    /// <c>true</c> if tournaments use fitness-weighted roulette wheel selection among tournament participants;
    /// <c>false</c> if tournaments deterministically select the two fittest individuals.
    /// </value>
    public bool StochasticTournament { get; } = stochasticTournament;

    /// <summary>
    /// The minimum tournament size as a percentage of the population (5%).
    /// Smaller tournaments reduce selection pressure and increase diversity.
    /// </summary>
    private const int _tournamentSizeMinPercentage = 5;

    /// <summary>
    /// The maximum tournament size as a percentage of the population (20%).
    /// Larger tournaments increase selection pressure and favor fitter individuals.
    /// </summary>
    private const int _tournamentSizeMaxPercentage = 21;

    /// <summary>
    /// Selects mating pairs from the population using tournament-based selection.
    /// Each couple is generated through independent tournaments of random individuals.
    /// </summary>
    /// <param name="population">The array of chromosomes available for mating</param>
    /// <param name="random">Random number generator for tournament selection and stochastic decisions</param>
    /// <param name="minimumNumberOfCouples">The minimum number of couples to generate</param>
    /// <returns>A collection of couples selected through tournament competition</returns>
    /// <exception cref="ArgumentNullException">Thrown when population or random is null</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when minimumNumberOfCouples is negative</exception>
    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        // Validate input parameters
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumNumberOfCouples);

        // Handle edge cases
        if (population.Length <= 1)
        {
            yield break; // Cannot form couples with 0 or 1 individuals
        }

        // Handle special case: exactly two individuals
        if (population.Length == 2)
        {
            foreach (var couple in GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples))
            {
                yield return couple;
            }
            
            yield break;
        }

        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            // Calculate tournament size as percentage of population (5-20%)
            var tournamentSizeAsPercentageOfPopulation = random.Next(_tournamentSizeMinPercentage, _tournamentSizeMaxPercentage);
            var tournamentSize = (int)Math.Ceiling(population.Length * (double)(tournamentSizeAsPercentageOfPopulation / 100));
            
            // Ensure tournament size is within valid bounds
            tournamentSize = Math.Min(population.Length, tournamentSize);

            // Handle edge case: if calculated tournament size is too small, use whole population
            if (tournamentSize <= 1)
            {
                if (population.Length > 1)
                {
                    tournamentSize = population.Length;
                }
                else
                {
                    continue; // Skip if only 1 individual in population
                }
            }

            // Create tournament by randomly selecting participants
            var tournament = population.FisherYatesShuffle(random).Take(tournamentSize).ToArray();

            // Select winners based on tournament strategy
            if (!StochasticTournament)
            {
                // Deterministic tournament: select the two fittest individuals
                var orderedTournament = tournament.OrderByDescending(x => x.Fitness).ToArray();
                yield return Couple<T>.Pair(orderedTournament[0], orderedTournament[1]);
            }
            else
            {
                // Stochastic tournament: use fitness-weighted selection
                var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(tournament, d => d.Fitness);
                var winner1 = rouletteWheel.SpinAndReadjustWheel(); // Remove winner1 from subsequent selections
                var winner2 = rouletteWheel.Spin(); // Select winner2 from remaining participants

                yield return Couple<T>.Pair(winner1, winner2);
            }
        }
    }
}
