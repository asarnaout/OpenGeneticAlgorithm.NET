using OpenGA.Net.Extensions;

namespace OpenGA.Net.SurvivorSelectionStrategies;

/// <summary>
/// A tournament-based survivor selection strategy that eliminates chromosomes through competitive tournaments.
/// Chromosomes compete in tournaments where the least fit individuals are more likely to be eliminated.
/// This strategy uses the WeightedRouletteWheel to add stochastic selection within tournaments.
/// </summary>
public class TournamentSurvivorSelectionStrategy<T> : BaseSurvivorSelectionStrategy<T>
{
    /// <summary>
    /// The recommended offspring generation rate for tournament survivor selection strategy.
    /// This moderate to high turnover rate (50%) works well with tournament selection's inherent fitness preservation.
    /// </summary>
    internal override float RecommendedOffspringGenerationRate => 0.5f;

    private readonly int _tournamentSize;
    private readonly bool _stochasticTournament;

    /// <summary>
    /// Initializes a new instance of the TournamentSurvivorSelectionStrategy.
    /// </summary>
    /// <param name="tournamentSize">
    /// The number of chromosomes that participate in each tournament. Must be at least 2.
    /// Larger tournaments increase selection pressure towards eliminating less fit chromosomes.
    /// </param>
    /// <param name="stochasticTournament">
    /// If true, uses weighted random selection within tournaments based on inverse fitness.
    /// If false, always eliminates the least fit chromosome in each tournament.
    /// </param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Thrown when tournamentSize is less than 2.
    /// </exception>
    public TournamentSurvivorSelectionStrategy(int tournamentSize, bool stochasticTournament = false)
    {
        if (tournamentSize < 2)
        {
            throw new ArgumentOutOfRangeException(
                nameof(tournamentSize), 
                tournamentSize, 
                "Tournament size must be at least 2.");
        }

        _tournamentSize = tournamentSize;
        _stochasticTournament = stochasticTournament;
    }

    /// <summary>
    /// Selects chromosomes for elimination using tournament selection.
    /// Chromosomes are grouped into tournaments where the least fit individuals are eliminated.
    /// The number of eliminations is determined by the total number of chromosomes that need to be eliminated
    /// to accommodate the offspring.
    /// </summary>
    /// <param name="population">The current population of chromosomes</param>
    /// <param name="offspring">The newly generated offspring chromosomes</param>
    /// <param name="random">Random number generator for stochastic operations</param>
    /// <param name="currentEpoch">The current epoch/generation number (not used in tournament elimination)</param>
    /// <returns>The chromosomes selected for elimination through tournament competition</returns>
    protected internal override async Task<IEnumerable<Chromosome<T>>> SelectChromosomesForEliminationAsync(
        Chromosome<T>[] population, 
        Chromosome<T>[] offspring, 
        Random random,
        int currentEpoch = 0)
    {
        if (population.Length == 0 || offspring.Length == 0)
        {
            return [];
        }

        // We need to eliminate as many chromosomes as we have offspring
        var eliminationsNeeded = Math.Min(offspring.Length, population.Length);

        var candidatesForElimination = new List<Chromosome<T>>(eliminationsNeeded);
        
        // Use HashSet for O(1) lookups instead of List.Contains which is O(n)
        var eliminatedChromosomes = new HashSet<Chromosome<T>>();
        
        // Pre-shuffle the population once using extension method instead of calling OrderBy(random.Next()) repeatedly
        var shuffledPopulation = population.FisherYatesShuffle(random);

        int currentIndex = 0;

        // Run tournaments until we have enough eliminations
        while (candidatesForElimination.Count < eliminationsNeeded)
        {
            // Check if we have enough remaining chromosomes for a tournament
            var availableCount = population.Length - eliminatedChromosomes.Count;
            if (availableCount < _tournamentSize)
            {
                break;
            }

            // Select tournament participants more efficiently
            var tournamentParticipants = new List<Chromosome<T>>(_tournamentSize);
            int participantsFound = 0;
            
            // Use circular buffer approach to find available chromosomes
            for (int attempts = 0; attempts < population.Length && participantsFound < _tournamentSize; attempts++)
            {
                var candidate = shuffledPopulation[currentIndex];
                currentIndex = (currentIndex + 1) % shuffledPopulation.Length;
                
                if (!eliminatedChromosomes.Contains(candidate))
                {
                    tournamentParticipants.Add(candidate);
                    participantsFound++;
                }
            }

            if (tournamentParticipants.Count < _tournamentSize)
            {
                break;
            }

            Chromosome<T> loser;

            if (_stochasticTournament)
            {
                // Use weighted roulette wheel with inverse fitness (lower fitness = higher chance of elimination)
                loser = await SelectLoserStochasticallyAsync(tournamentParticipants);
            }
            else
            {
                // Deterministic: always eliminate the least fit
                loser = await GetLeastFitChromosomeAsync(tournamentParticipants);
            }

            candidatesForElimination.Add(loser);
            eliminatedChromosomes.Add(loser);
        }

        // If we still need more eliminations, randomly select from remaining chromosomes
        while (candidatesForElimination.Count < eliminationsNeeded && eliminatedChromosomes.Count < population.Length)
        {
            var candidate = shuffledPopulation[currentIndex];
            currentIndex = (currentIndex + 1) % shuffledPopulation.Length;
            
            if (!eliminatedChromosomes.Contains(candidate))
            {
                candidatesForElimination.Add(candidate);
                eliminatedChromosomes.Add(candidate);
            }
        }

        return candidatesForElimination;
    }

    /// <summary>
    /// Selects a loser from tournament participants using weighted random selection based on inverse fitness.
    /// Chromosomes with lower fitness have higher probability of being eliminated.
    /// </summary>
    /// <param name="participants">The chromosomes participating in the tournament</param>
    /// <returns>The chromosome selected for elimination</returns>
    private static async Task<Chromosome<T>> SelectLoserStochasticallyAsync(IList<Chromosome<T>> participants)
    {
        if (participants.Count == 1)
        {
            return participants[0];
        }

        // Get fitness values for all participants
        var fitnessValues = new double[participants.Count];
        for (int i = 0; i < participants.Count; i++)
        {
            fitnessValues[i] = await participants[i].GetCachedFitnessAsync();
        }

        // Calculate inverse fitness weights (lower fitness = higher elimination probability)
        var maxFitness = fitnessValues.Max();
        var minFitness = fitnessValues.Min();
        
        // Add small epsilon to avoid division by zero and ensure all have some elimination probability
        var epsilon = (maxFitness - minFitness) * 0.01 + 0.001;

        // Create a dictionary mapping chromosomes to their fitness values for O(1) lookup
        var fitnessLookup = new Dictionary<Chromosome<T>, double>();
        for (int i = 0; i < participants.Count; i++)
        {
            fitnessLookup[participants[i]] = fitnessValues[i];
        }

        // Create weighted roulette wheel with inverse fitness
        var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(participants, chromosome =>
        {
            // Inverse weight: maxFitness + epsilon - fitness gives higher weight to lower fitness
            return maxFitness + epsilon - fitnessLookup[chromosome];
        });

        return rouletteWheel.Spin();
    }

    /// <summary>
    /// Efficiently finds the chromosome with the lowest fitness from the participants.
    /// </summary>
    /// <param name="participants">The chromosomes to evaluate</param>
    /// <returns>The chromosome with the lowest fitness</returns>
    private static async Task<Chromosome<T>> GetLeastFitChromosomeAsync(IList<Chromosome<T>> participants)
    {
        var leastFit = participants[0];
        var minFitness = await leastFit.GetCachedFitnessAsync();

        for (int i = 1; i < participants.Count; i++)
        {
            var fitness = await participants[i].GetCachedFitnessAsync();
            if (fitness < minFitness)
            {
                minFitness = fitness;
                leastFit = participants[i];
            }
        }

        return leastFit;
    }

    /// <summary>
    /// Gets the tournament size used by this survivor selection strategy.
    /// </summary>
    public int TournamentSize => _tournamentSize;

    /// <summary>
    /// Gets whether this strategy uses stochastic tournament selection.
    /// </summary>
    public bool StochasticTournament => _stochasticTournament;
}
