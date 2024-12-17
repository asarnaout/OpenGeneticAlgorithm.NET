namespace OpenGA.Net.CrossoverSelectors;

public class ElitistCrossoverSelector<T> : BaseCrossoverSelector<T>
{
    private int _requiredNumberOfCouples;

    public override IEnumerable<Couple<T>> SelectParents(Chromosome<T>[] population, CrossoverConfiguration config, Random random, int minimumNumberOfCouples)
    {
        if (population.Length <= 1)
        {
            return [];
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        var populationSortedByFitness = population.OrderByDescending(x => x.CalculateFitness());

        var numberOfElitesInCurrentPopulation = (int)Math.Ceiling(config.ProportionOfElitesInPopulation * population.Length);
        
        var eliteCandidates = populationSortedByFitness.Take(numberOfElitesInCurrentPopulation).ToList();
        
        var eliteIdentifiers = eliteCandidates.Select(x => x.InternalIdentifier).ToHashSet();
        
        IList<Chromosome<T>> nonEliteCandidates = [];

        var eligibleCandidatesForPhase1 = eliteCandidates;

        if (config.ProportionOfNonElitesAllowedToMate > 0)
        {
            nonEliteCandidates = population
                                    .Where(x => !eliteIdentifiers.Contains(x.InternalIdentifier))
                                    .OrderBy(x => random.Next())
                                    .Take((int)Math.Ceiling(config.ProportionOfNonElitesAllowedToMate * (population.Length - eliteCandidates.Count)))
                                    .ToList();

            if (config.AllowMatingElitesWithNonElites)
            {
                eligibleCandidatesForPhase1 = [.. eligibleCandidatesForPhase1, .. nonEliteCandidates];
            }
        }

        _requiredNumberOfCouples = minimumNumberOfCouples;

        var phase1Couples = SelectAllElitesForMating(random, eliteCandidates, eligibleCandidatesForPhase1, eliteIdentifiers);

        List<Chromosome<T>> eligibleCandidatesForPhase2 = [];

        if (eliteCandidates.Count > 1 || (eliteCandidates.Count == 1 && nonEliteCandidates.Count >= 1 && config.AllowMatingElitesWithNonElites))
        {
            eligibleCandidatesForPhase2.AddRange(eliteCandidates);
        }

        if (nonEliteCandidates.Count > 1 || (nonEliteCandidates.Count == 1 && config.AllowMatingElitesWithNonElites))
        {
            eligibleCandidatesForPhase2.AddRange(nonEliteCandidates);
        }

        var phase2Couples = SupplementRequiredNumberOfCouplesWithExtraPairs(config, eligibleCandidatesForPhase2, eliteIdentifiers, eliteCandidates, nonEliteCandidates);
        
        return [.. phase1Couples, .. phase2Couples];
    }

    /// <summary>
    /// Phase 1: Ensure that every elite has had a chance to mate (as long as there is at least one more eligible individual to mate with).
    /// The method allows mating elites with non elites if allowMatingElitesWithNonElites is set to true. 
    /// </summary>
    private IEnumerable<Couple<T>> SelectAllElitesForMating(Random random, IList<Chromosome<T>> eliteCandidates, List<Chromosome<T>> eligibleCandidatesForPhase1, HashSet<Guid> eliteIdentifiers)
    {
        var singleElites = new List<Chromosome<T>>(eliteCandidates.OrderBy(x => random.Next()));

        while (singleElites.Count > 0)
        {
            var parent1 = singleElites[0];

            var pool = eligibleCandidatesForPhase1.Where(x => x != parent1).ToList();

            if (pool.Count == 0) //If elites can ONLY mate with other elites, then ensure that at least 2 elites are present
            {
                break;
            }

            var parent2 = WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(pool).Spin();

            _requiredNumberOfCouples--;

            if (eliteIdentifiers.Contains(parent2.InternalIdentifier))
            {
                singleElites.Remove(parent2);
            }

            singleElites.Remove(parent1);

            yield return Couple<T>.Pair(parent1, parent2);
        }
    }

    /// <summary>
    /// Phase 2: Now that the requirement of mating all elites has been fulfilled, we would like to generate more couples
    /// out of the existing population of elites (and non-elites, if any).
    /// The method allows mating elites with non elites if allowMatingElitesWithNonElites is set to true. 
    /// </summary>
    private IEnumerable<Couple<T>> SupplementRequiredNumberOfCouplesWithExtraPairs(CrossoverConfiguration config, List<Chromosome<T>> eligibleCandidatesForPhase2, HashSet<Guid> eliteIdentifiers, IList<Chromosome<T>> eliteCandidates, IList<Chromosome<T>> nonEliteCandidates)
    {
        for (var i = 0; i < _requiredNumberOfCouples; i++)
        {
            if (eligibleCandidatesForPhase2.Count <= 1)
            {
                break;
            }

            var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(eligibleCandidatesForPhase2);

            var winner1 = rouletteWheel.SpinAndReadjustWheel();

            if (config.ProportionOfNonElitesAllowedToMate > 0 && !config.AllowMatingElitesWithNonElites)
            {
                var isEliteWinner = eliteIdentifiers.Contains(winner1.InternalIdentifier);

                if (isEliteWinner)
                {
                    rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(eliteCandidates.Where(x => x.InternalIdentifier != winner1.InternalIdentifier).ToList());
                }
                else
                {
                    rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.InitWithUniformWeights(nonEliteCandidates.Where(x => x.InternalIdentifier != winner1.InternalIdentifier).ToList());
                }
            }

            var winner2 = rouletteWheel.SpinAndReadjustWheel();

            yield return Couple<T>.Pair(winner1, winner2);
        }
    }
}
