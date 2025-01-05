namespace OpenGA.Net.ReproductionSelectors;

public class TournamentReproductionSelector<T>(bool stochasticTournament) : BaseReproductionSelector<T>
{
    public bool StochasticTournament { get; } = stochasticTournament;

    private const int _tournamentSizeMinPercentage = 5;

    private const int _tournamentSizeMaxPercentage = 21;

    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        for (var i = 0; i < minimumNumberOfCouples; i++)
        {
            var tournamentSizeAsPercentageOfPopulation = random.Next(_tournamentSizeMinPercentage, _tournamentSizeMaxPercentage); //Tournament size is between 5 to 20% of the population, larger tournaments force selection pressure that favors more fit individuals.

            var tournamentSize = (int)Math.Ceiling(population.Length * (double)(tournamentSizeAsPercentageOfPopulation / 100));

            tournamentSize = Math.Min(population.Length, tournamentSize); //Tournament cannot be larger than the population size

            if (tournamentSize <= 1)
            {
                if (population.Length > 1)
                {
                    tournamentSize = population.Length;
                }
                else
                {
                    continue;
                }
            }

            if (tournamentSize == 2)
            {
                yield return Couple<T>.Pair(population[0], population[1]);
                continue;
            }
            
            var tournament = population.OrderBy(x => random.Next()).Take(tournamentSize).ToList();

            if (!StochasticTournament)
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
