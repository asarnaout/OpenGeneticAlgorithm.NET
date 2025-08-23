using OpenGA.Net;
using OpenGA.Net.Examples;

//WIP
var runner = OpenGARunner<float>
                .Init([new ExampleChromosome([1, 2, 3])])
                .Epochs(60)
                .MaxPopulationSize(100)
                .MutationRate(0.25f)
                .ApplyReproductionSelectors(c => c.ApplyRandomReproductionSelector(),
                                            c => c.ApplyElitistReproductionSelector(),
                                            c => c.ApplyRankSelectionReproductionSelector()
                )
                .ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())
                // Replacement strategy options:
                // c.ApplyRandomEliminationReplacementStrategy() - Randomly eliminates chromosomes
                // c.ApplyTournamentReplacementStrategy() - Tournament-based elimination
                // c.ApplyGenerationalReplacementStrategy() - Complete population replacement with offspring
                // c.ApplyElitistReplacementStrategy(0.1f) - Protects top 10% (elites) from elimination
                .ApplyReplacementStrategy(c => c.ApplyTournamentReplacementStrategy());
                