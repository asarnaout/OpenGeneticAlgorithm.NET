using OpenGA.Net;
using OpenGA.Net.Examples;

//WIP
var runner = OpenGARunner<float>
                .Init([new ExampleChromosome([1, 2, 3])])
                .Epochs(60)
                .MaxPopulationSize(100)
                .MutationRate(0.25f)
                .ApplyReproductionSelector(c => c.RankSelection())
                .ApplyCrossoverStrategy(c => c.OnePointCrossover())
                .ApplyEliminationMechanism(c => c.Elitism());