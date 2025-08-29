namespace OpenGA.Net.ParentSelectorStrategies;

public class ElitistParentSelectorStrategy<T>(bool allowMatingElitesWithNonElites, float proportionOfElitesInPopulation, float proportionOfNonElitesAllowedToMate) : BaseParentSelectorStrategy<T>
{
    internal bool AllowMatingElitesWithNonElites { get; } = allowMatingElitesWithNonElites;
    internal float ProportionOfNonElitesAllowedToMate { get; } = proportionOfNonElitesAllowedToMate;
    internal float ProportionOfElitesInPopulation { get; } = proportionOfElitesInPopulation;

    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }
        
        // Get fitness values for all chromosomes
        var populationWithFitness = new List<(Chromosome<T> chromosome, double fitness)>();
        foreach (var chromosome in population)
        {
            var fitness = await chromosome.GetCachedFitnessAsync();
            populationWithFitness.Add((chromosome, fitness));
        }
        
        var sortedPopulation = populationWithFitness.OrderByDescending(x => x.fitness).Select(x => x.chromosome).ToArray();
        
        var eliteCount = Math.Max(1, (int)Math.Ceiling(ProportionOfElitesInPopulation * population.Length));
        var elites = sortedPopulation.Take(eliteCount).ToHashSet();
        
        var nonEliteCount = ProportionOfNonElitesAllowedToMate > 0 
            ? (int)Math.Ceiling(ProportionOfNonElitesAllowedToMate * (population.Length - eliteCount))
            : 0;
        var nonElites = sortedPopulation.Skip(eliteCount).Take(nonEliteCount).ToArray();
        
        return await GenerateElitistCouplesAsync(elites, nonElites, minimumNumberOfCouples);
    }

    private async Task<IEnumerable<Couple<T>>> GenerateElitistCouplesAsync(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites, 
        int minimumNumberOfCouples)
    {
        var couples = new List<Couple<T>>();
        var matedChromosomes = new HashSet<Guid>();
        
        couples.AddRange(await MateAllElitesAsync(elites, nonElites, matedChromosomes));
        
        if (couples.Count < minimumNumberOfCouples)
        {
            var remainingCouples = minimumNumberOfCouples - couples.Count;
            couples.AddRange(await GenerateAdditionalCouplesAsync(elites, nonElites, remainingCouples));
        }
        
        return couples;
    }

    private async Task<IEnumerable<Couple<T>>> MateAllElitesAsync(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites,
        HashSet<Guid> matedChromosomes)
    {
        var couples = new List<Couple<T>>();
        var unmatedElites = new Queue<Chromosome<T>>(elites);
        
        while (unmatedElites.Count > 0)
        {
            var elite = unmatedElites.Dequeue();
            
            var potentialMates = GetPotentialMates(elite, elites, nonElites, matedChromosomes);
            
            if (potentialMates.Length == 0)
            {
                potentialMates = GetPotentialMates(elite, elites, nonElites, []);
            }
            
            if (potentialMates.Length == 0)
            {
                break;
            }
            
            var mate = await SelectMateByFitnessAsync(potentialMates);
            
            couples.Add(Couple<T>.Pair(elite, mate));
            
            matedChromosomes.Add(elite.InternalIdentifier);
        }
        
        return couples;
    }

    private async Task<IEnumerable<Couple<T>>> GenerateAdditionalCouplesAsync(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites,
        int couplesNeeded)
    {
        var couples = new List<Couple<T>>();
        var allCandidates = CreateMatingPool(elites, nonElites);
        
        for (int i = 0; i < couplesNeeded && allCandidates.Length >= 2; i++)
        {
            var parent1 = await SelectMateByFitnessAsync(allCandidates);
            
            var potentialMates = GetPotentialMates(parent1, elites, nonElites, []);
            
            if (potentialMates.Length == 0)
            {
                break;
            }
            
            var parent2 = await SelectMateByFitnessAsync(potentialMates);
            
            couples.Add(Couple<T>.Pair(parent1, parent2));
        }
        
        return couples;
    }

    private Chromosome<T>[] CreateMatingPool(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites)
    {
        var pool = new List<Chromosome<T>>();
        
        if (elites.Count >= 2)
        {
            pool.AddRange(elites);
        }
        
        if (AllowMatingElitesWithNonElites)
        {
            pool.AddRange(nonElites);
            
            if (elites.Count == 1 && nonElites.Length > 0)
            {
                pool.Add(elites.First());
            }
        }
        else
        {
            if (nonElites.Length >= 2)
            {
                pool.AddRange(nonElites);
            }
        }
        
        return [.. pool];
    }

    private Chromosome<T>[] GetPotentialMates(Chromosome<T> chromosome, HashSet<Chromosome<T>> elites, 
        IEnumerable<Chromosome<T>> nonElites, HashSet<Guid> matedChromosomes)
    {
        var isElite = elites.Contains(chromosome);
        var potentialMates = new List<Chromosome<T>>();
        
        if (isElite)
        {
            potentialMates.AddRange(elites.Where(e => 
                e.InternalIdentifier != chromosome.InternalIdentifier && 
                !matedChromosomes.Contains(e.InternalIdentifier)));
            
            if (AllowMatingElitesWithNonElites)
            {
                potentialMates.AddRange(nonElites.Where(ne => 
                    !matedChromosomes.Contains(ne.InternalIdentifier)));
            }
        }
        else
        {
            potentialMates.AddRange(nonElites.Where(ne => 
                    ne.InternalIdentifier != chromosome.InternalIdentifier && 
                    !matedChromosomes.Contains(ne.InternalIdentifier)));

            if (AllowMatingElitesWithNonElites)
            {
                potentialMates.AddRange(elites.Where(e =>
                    !matedChromosomes.Contains(e.InternalIdentifier)));
            }
        }
        
        return [.. potentialMates];
    }

    private static async Task<Chromosome<T>> SelectMateByFitnessAsync(Chromosome<T>[] candidates)
    {
        if (candidates.Length == 1)
        {
            return candidates[0];
        }
        
        // Get fitness values for all candidates
        var fitnessValues = new double[candidates.Length];
        for (int i = 0; i < candidates.Length; i++)
        {
            fitnessValues[i] = await candidates[i].GetCachedFitnessAsync();
        }
        
        // Create a dictionary mapping chromosomes to their fitness values for O(1) lookup
        var fitnessLookup = new Dictionary<Chromosome<T>, double>();
        for (int i = 0; i < candidates.Length; i++)
        {
            fitnessLookup[candidates[i]] = fitnessValues[i];
        }
        
        var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(candidates, c => fitnessLookup[c]);
        return rouletteWheel.Spin();
    }
}
