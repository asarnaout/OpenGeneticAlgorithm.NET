namespace OpenGA.Net.ParentSelectors;

public class ElitistParentSelector<T>(bool allowMatingElitesWithNonElites, float proportionOfElitesInPopulation, float proportionOfNonElitesAllowedToMate) : BaseParentSelector<T>
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
        
        var sortedPopulation = population.OrderByDescending(x => x.Fitness).ToArray();
        
        var eliteCount = Math.Max(1, (int)Math.Ceiling(ProportionOfElitesInPopulation * population.Length));
        var elites = sortedPopulation.Take(eliteCount).ToHashSet();
        
        var nonEliteCount = ProportionOfNonElitesAllowedToMate > 0 
            ? (int)Math.Ceiling(ProportionOfNonElitesAllowedToMate * (population.Length - eliteCount))
            : 0;
        var nonElites = sortedPopulation.Skip(eliteCount).Take(nonEliteCount).ToArray();
        
        return GenerateElitistCouples(elites, nonElites, minimumNumberOfCouples);
    }

    private IEnumerable<Couple<T>> GenerateElitistCouples(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites, 
        int minimumNumberOfCouples)
    {
        var couples = new List<Couple<T>>();
        var matedChromosomes = new HashSet<Guid>();
        
        couples.AddRange(MateAllElites(elites, nonElites, matedChromosomes));
        
        if (couples.Count < minimumNumberOfCouples)
        {
            var remainingCouples = minimumNumberOfCouples - couples.Count;
            couples.AddRange(GenerateAdditionalCouples(elites, nonElites, remainingCouples));
        }
        
        return couples;
    }

    private IEnumerable<Couple<T>> MateAllElites(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites,
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
            
            var mate = SelectMateByFitness(potentialMates);
            
            couples.Add(Couple<T>.Pair(elite, mate));
            
            matedChromosomes.Add(elite.InternalIdentifier);
        }
        
        return couples;
    }

    private IEnumerable<Couple<T>> GenerateAdditionalCouples(HashSet<Chromosome<T>> elites, Chromosome<T>[] nonElites,
        int couplesNeeded)
    {
        var couples = new List<Couple<T>>();
        var allCandidates = CreateMatingPool(elites, nonElites);
        
        for (int i = 0; i < couplesNeeded && allCandidates.Length >= 2; i++)
        {
            var parent1 = SelectMateByFitness(allCandidates);
            
            var potentialMates = GetPotentialMates(parent1, elites, nonElites, []);
            
            if (potentialMates.Length == 0)
            {
                break;
            }
            
            var parent2 = SelectMateByFitness(potentialMates);
            
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

    private static Chromosome<T> SelectMateByFitness(Chromosome<T>[] candidates)
    {
        if (candidates.Length == 1)
        {
            return candidates[0];
        }
        
        var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(candidates, c => c.Fitness);
        return rouletteWheel.Spin();
    }
}
