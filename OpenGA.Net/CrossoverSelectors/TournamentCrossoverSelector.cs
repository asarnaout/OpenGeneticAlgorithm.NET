namespace OpenGA.Net.CrossoverSelectors;

public class TournamentCrossoverSelector<T> : BaseCrossoverSelector<T>
{
    public override IEnumerable<Couple<T>> SelectParents(Chromosome<T>[] population, CrossoverConfiguration config, Random random, int minimumNumberOfCouples)
    {
        config.TournamentSize = Math.Min(population.Length, config.TournamentSize); //Tournament cannot be larger than the population size

        if (config.TournamentSize <= 1)
        {
            yield break;
        }

        if (config.TournamentSize == 2)
        {
            for (var i = 0; i < minimumNumberOfCouples; i++)
            {
                yield return Couple<T>.Pair(population[0], population[1]);
            }

            yield break;
        }

        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            var tournament = population.OrderBy(x => random.Next()).Take(config.TournamentSize).ToList();

            if (!config.StochasticTournament)
            {
                var orderedTournament = tournament.OrderByDescending(x => x.CalculateFitness()).ToList();
                yield return Couple<T>.Pair(orderedTournament[0], orderedTournament[1]);
            }
            else
            {
                var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(tournament, d => d.CalculateFitness());
                var winner1 = rouletteWheel.SpinAndReadjustWheel();
                var winner2 = rouletteWheel.Spin();

                yield return Couple<T>.Pair(winner1, winner2);
            }
        }
    }
}
