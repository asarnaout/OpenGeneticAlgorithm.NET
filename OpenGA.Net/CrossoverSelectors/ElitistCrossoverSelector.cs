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
        
        var eliteCandidates = population.OrderByDescending(x => x.CalculateFitness())
                                        .Take((int)Math.Ceiling(config.ProportionOfElitesInPopulation * population.Length))
                                        .ToList();
        
        var eliteIdentifiers = eliteCandidates.Select(x => x.InternalIdentifier).ToHashSet();
        
        var nonEliteCandidates = config.ProportionOfNonElitesAllowedToMate > 0? population
                                        .Where(x => !eliteIdentifiers.Contains(x.InternalIdentifier))
                                        .OrderBy(x => random.Next())
                                        .Take((int)Math.Ceiling(config.ProportionOfNonElitesAllowedToMate * (population.Length - eliteCandidates.Count)))
                                        .ToList() : [];

        _requiredNumberOfCouples = minimumNumberOfCouples;

        var phase1Couples = SelectAllElitesForMating(config, eliteIdentifiers, eliteCandidates, nonEliteCandidates);

        var phase2Couples = GenerateAdditionalPairs(config, eliteIdentifiers, eliteCandidates, nonEliteCandidates);
        
        return [.. phase1Couples, .. phase2Couples];
    }

    /// <summary>
    /// Phase 1: Ensure that every elite has had a chance to mate (as long as there is at least one more eligible individual to mate with).
    /// The method allows mating elites with non elites if allowMatingElitesWithNonElites is set to true. 
    /// </summary>
    private IEnumerable<Couple<T>> SelectAllElitesForMating(CrossoverConfiguration config, HashSet<Guid> eliteIdentifiers, IList<Chromosome<T>> eliteCandidates, IList<Chromosome<T>> nonEliteCandidates)
    {
        var eligibleCandidatesForPhase1 = config.AllowMatingElitesWithNonElites
            ? [.. eliteCandidates, .. nonEliteCandidates]
            : eliteCandidates.ToList();

        var singleElites = new HashSet<Chromosome<T>>(eliteCandidates);

        while (singleElites.Count > 0)
        {
            var parent1 = singleElites.First();

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
    private IEnumerable<Couple<T>> GenerateAdditionalPairs(CrossoverConfiguration config, HashSet<Guid> eliteIdentifiers, IList<Chromosome<T>> eliteCandidates, IList<Chromosome<T>> nonEliteCandidates)
    {
        if (_requiredNumberOfCouples <= 0)
        {
            yield break;
        }

        List<Chromosome<T>> eligibleCandidatesForPhase2 = [];

        if (eliteCandidates.Count > 1 || (eliteCandidates.Count == 1 && nonEliteCandidates.Count >= 1 && config.AllowMatingElitesWithNonElites))
        {
            eligibleCandidatesForPhase2.AddRange(eliteCandidates);
        }

        if (nonEliteCandidates.Count > 1 || (nonEliteCandidates.Count == 1 && config.AllowMatingElitesWithNonElites))
        {
            eligibleCandidatesForPhase2.AddRange(nonEliteCandidates);
        }

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
