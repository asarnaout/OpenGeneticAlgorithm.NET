using OpenGA.Net.Extensions;

namespace OpenGA.Net.ParentSelectorStrategies;

public class TournamentParentSelectorStrategy<T>(bool stochasticTournament) : BaseParentSelectorStrategy<T>
{
    public bool StochasticTournament { get; } = stochasticTournament;

    private const int _tournamentSizeMinPercentage = 5;
    private const int _tournamentSizeMaxPercentage = 21;

    protected internal override IEnumerable<Couple<T>> SelectMatingPairs(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumNumberOfCouples);

        if (population.Length <= 1)
        {
            yield break;
        }

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
            var tournamentSizeAsPercentageOfPopulation = random.Next(_tournamentSizeMinPercentage, _tournamentSizeMaxPercentage);
            var tournamentSize = (int)Math.Ceiling(population.Length * (double)(tournamentSizeAsPercentageOfPopulation / 100));
            
            tournamentSize = Math.Min(population.Length, tournamentSize);

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

            var tournament = population.FisherYatesShuffle(random).Take(tournamentSize).ToArray();

            if (!StochasticTournament)
            {
                var orderedTournament = tournament.OrderByDescending(x => x.Fitness).ToArray();
                yield return Couple<T>.Pair(orderedTournament[0], orderedTournament[1]);
            }
            else
            {
                var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(tournament, d => d.Fitness);
                var winner1 = rouletteWheel.SpinAndReadjustWheel();
                var winner2 = rouletteWheel.Spin();

                yield return Couple<T>.Pair(winner1, winner2);
            }
        }
    }
}
