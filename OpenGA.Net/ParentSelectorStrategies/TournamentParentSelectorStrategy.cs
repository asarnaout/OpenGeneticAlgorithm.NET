using OpenGA.Net.Extensions;

namespace OpenGA.Net.ParentSelectorStrategies;

public class TournamentParentSelectorStrategy<T>(bool stochasticTournament) : BaseParentSelectorStrategy<T>
{
    public bool StochasticTournament { get; } = stochasticTournament;

    private const int _tournamentSizeMinPercentage = 5;
    private const int _tournamentSizeMaxPercentage = 21;

    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        ArgumentNullException.ThrowIfNull(population);
        ArgumentNullException.ThrowIfNull(random);
        ArgumentOutOfRangeException.ThrowIfNegative(minimumNumberOfCouples);

        if (population.Length <= 1)
        {
            return new List<Couple<T>>();
        }

        if (population.Length == 2)
        {
            return GenerateCouplesFromATwoIndividualPopulation(population, minimumNumberOfCouples);
        }

        var couples = new List<Couple<T>>();

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
                // Get fitness values for all tournament participants
                var tournamentWithFitness = new List<(Chromosome<T> chromosome, double fitness)>();
                foreach (var chromosome in tournament)
                {
                    var fitness = await chromosome.GetCachedFitnessAsync();
                    tournamentWithFitness.Add((chromosome, fitness));
                }

                var orderedTournament = tournamentWithFitness.OrderByDescending(x => x.fitness).ToArray();
                couples.Add(Couple<T>.Pair(orderedTournament[0].chromosome, orderedTournament[1].chromosome));
            }
            else
            {
                // Get fitness values for roulette wheel
                var fitnessValues = new double[tournament.Length];
                for (int j = 0; j < tournament.Length; j++)
                {
                    fitnessValues[j] = await tournament[j].GetCachedFitnessAsync();
                }

                var rouletteWheel = WeightedRouletteWheel<Chromosome<T>>.Init(tournament, (chromosome) => 
                {
                    var index = Array.IndexOf(tournament, chromosome);
                    return fitnessValues[index];
                });
                var winner1 = rouletteWheel.SpinAndReadjustWheel();
                var winner2 = rouletteWheel.Spin();

                couples.Add(Couple<T>.Pair(winner1, winner2));
            }
        }

        return couples;
    }
}
