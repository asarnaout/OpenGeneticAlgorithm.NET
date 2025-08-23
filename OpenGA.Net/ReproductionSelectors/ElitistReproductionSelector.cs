namespace OpenGA.Net.ReproductionSelectors;

/// <summary>
/// An elitist reproduction selector that prioritizes the fittest individuals (elites) for mating.
/// This strategy ensures that the best chromosomes in the population have the highest probability
/// of reproducing, while still allowing some diversity through controlled non-elite participation.
/// </summary>
public class ElitistReproductionSelector<T>(bool allowMatingElitesWithNonElites, float proportionOfElitesInPopulation, float proportionOfNonElitesAllowedToMate) : BaseReproductionSelector<T>
{
    internal bool AllowMatingElitesWithNonElites { get; } = allowMatingElitesWithNonElites;
    internal float ProportionOfNonElitesAllowedToMate { get; } = proportionOfNonElitesAllowedToMate;
    internal float ProportionOfElitesInPopulation { get; } = proportionOfElitesInPopulation;

    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }
        
        // Sort population by fitness (best first)
        var sortedPopulation = population.OrderByDescending(x => x.Fitness).ToArray();
        
        // Determine elite and non-elite groups
        var eliteCount = Math.Max(1, (int)Math.Ceiling(ProportionOfElitesInPopulation * population.Length));
        var elites = sortedPopulation.Take(eliteCount).ToHashSet();
        
        var nonEliteCount = ProportionOfNonElitesAllowedToMate > 0 
            ? (int)Math.Ceiling(ProportionOfNonElitesAllowedToMate * (population.Length - eliteCount))
            : 0;
        var nonElites = sortedPopulation.Skip(eliteCount).Take(nonEliteCount).ToArray();
        
        // Generate couples using elitist strategy
        return GenerateElitistCouples(elites, nonElites, minimumNumberOfCouples);
    }

    /// <summary>
    /// Generates couples using elitist principles:
    /// 1. All elites must participate in mating (when possible)
    /// 2. Use fitness-weighted selection to favor fitter individuals
    /// 3. Respect mating restrictions between elites and non-elites
    /// </summary>
    private IEnumerable<Couple<T>> GenerateElitistCouples(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites, 
        int minimumNumberOfCouples)
    {
        var couples = new List<Couple<T>>();
        var matedChromosomes = new HashSet<Guid>();
        
        // Phase 1: Ensure all elites get to mate at least once (elitist principle)
        // This happens regardless of minimumNumberOfCouples because elites should always reproduce
        couples.AddRange(MateAllElites(elites, nonElites, matedChromosomes));
        
        // Phase 2: Generate additional couples if needed to meet minimum requirement
        if (couples.Count < minimumNumberOfCouples)
        {
            var remainingCouples = minimumNumberOfCouples - couples.Count;
            couples.AddRange(GenerateAdditionalCouples(elites, nonElites, remainingCouples));
        }
        
        return couples;
    }

    /// <summary>
    /// Ensures every elite chromosome gets a chance to mate.
    /// This is the core of elitist strategy - elites must reproduce.
    /// </summary>
    private IEnumerable<Couple<T>> MateAllElites(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites,
        HashSet<Guid> matedChromosomes)
    {
        var couples = new List<Couple<T>>();
        var unmatedElites = new Queue<Chromosome<T>>(elites);
        
        while (unmatedElites.Count > 0)
        {
            var elite = unmatedElites.Dequeue();
            
            // Find potential mates for this elite (excluding already mated chromosomes for first round)
            var potentialMates = GetPotentialMates(elite, elites, nonElites, matedChromosomes);
            
            if (potentialMates.Length == 0)
            {
                // If no unmated partners available, allow mating with already mated chromosomes
                potentialMates = GetPotentialMates(elite, elites, nonElites, []);
            }
            
            if (potentialMates.Length == 0)
            {
                break; // Still no mates available
            }
            
            // Select mate using fitness-weighted selection
            var mate = SelectMateByFitness(potentialMates);
            
            couples.Add(Couple<T>.Pair(elite, mate));
            
            // Mark elite as mated (but not necessarily the mate, to allow multiple matings)
            matedChromosomes.Add(elite.InternalIdentifier);
        }
        
        return couples;
    }

    /// <summary>
    /// Generates additional couples after all elites have mated.
    /// Uses fitness-weighted selection while respecting mating rules.
    /// </summary>
    private IEnumerable<Couple<T>> GenerateAdditionalCouples(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites,
        int couplesNeeded)
    {
        var couples = new List<Couple<T>>();
        var allCandidates = CreateMatingPool(elites, nonElites);
        
        for (int i = 0; i < couplesNeeded && allCandidates.Length >= 2; i++)
        {
            // Select first parent using fitness-weighted selection
            var parent1 = SelectMateByFitness(allCandidates);
            
            // Get potential mates for parent1
            var potentialMates = GetPotentialMates(parent1, elites, nonElites, []);
            
            if (potentialMates.Length == 0)
            {
                break;
            }
            
            // Select second parent
            var parent2 = SelectMateByFitness(potentialMates);
            
            couples.Add(Couple<T>.Pair(parent1, parent2));
        }
        
        return couples;
    }

    /// <summary>
    /// Creates the mating pool based on configuration.
    /// Only includes chromosomes that can actually participate in mating.
    /// </summary>
    private Chromosome<T>[] CreateMatingPool(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites)
    {
        var pool = new List<Chromosome<T>>();
        
        // Always include elites if there are enough for mating
        if (elites.Count >= 2)
        {
            pool.AddRange(elites);
        }
        
        // Include non-elites based on configuration
        if (AllowMatingElitesWithNonElites)
        {
            // If cross-breeding allowed, add all non-elites
            pool.AddRange(nonElites);
            
            // Also add single elite if it can mate with non-elites
            if (elites.Count == 1 && nonElites.Length > 0)
            {
                pool.Add(elites.First());
            }
        }
        else
        {
            // If cross-breeding not allowed, only add non-elites if they can mate among themselves
            if (nonElites.Length >= 2)
            {
                pool.AddRange(nonElites);
            }
        }
        
        return [.. pool];
    }

    /// <summary>
    /// Gets potential mates for a given chromosome based on elitist mating rules.
    /// </summary>
    private Chromosome<T>[] GetPotentialMates(Chromosome<T> chromosome, HashSet<Chromosome<T>> elites, 
        IEnumerable<Chromosome<T>> nonElites, HashSet<Guid> matedChromosomes)
    {
        var isElite = elites.Contains(chromosome);
        var potentialMates = new List<Chromosome<T>>();
        
        if (isElite)
        {
            // Elite chromosome - can mate with other elites
            potentialMates.AddRange(elites.Where(e => 
                e.InternalIdentifier != chromosome.InternalIdentifier && 
                !matedChromosomes.Contains(e.InternalIdentifier)));
            
            // Can also mate with non-elites if allowed
            if (AllowMatingElitesWithNonElites)
            {
                potentialMates.AddRange(nonElites.Where(ne => 
                    !matedChromosomes.Contains(ne.InternalIdentifier)));
            }
        }
        else
        {
            // Non-elite chromosome
            potentialMates.AddRange(nonElites.Where(ne => 
                    ne.InternalIdentifier != chromosome.InternalIdentifier && 
                    !matedChromosomes.Contains(ne.InternalIdentifier)));

            if (AllowMatingElitesWithNonElites)
            {
                // Can mate with elites and other non-elites
                potentialMates.AddRange(elites.Where(e =>
                    !matedChromosomes.Contains(e.InternalIdentifier)));
            }
        }
        
        return [.. potentialMates];
    }

    /// <summary>
    /// Selects a mate using fitness-weighted roulette wheel selection.
    /// Higher fitness chromosomes have higher probability of selection.
    /// </summary>
    private static Chromosome<T> SelectMateByFitness(Chromosome<T>[] candidates)
    {
        if (candidates.Length == 1)
        {
            return candidates[0];
        }
        
        // Use fitness-weighted selection to favor fitter individuals
        var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(candidates, c => c.Fitness);
        return rouletteWheel.Spin();
    }
}
