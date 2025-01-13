using OpenGA.Net;
using OpenGA.Net.Examples;

//WIP
var runner = OpenGARunner<float>
                .Init([new ExampleChromosome([1, 2, 3])])
                .Epochs(60)
                .MaxPopulationSize(100)
                .MutationRate(0.25f)
                .ApplyReproductionSelectors(c => c.ApplyRandomReproductionSelector().Weight(0.6f),
                                            c => c.ApplyElitistReproductionSelector().Weight(0.3f),
                                            c => c.ApplyRankSelectionReproductionSelector().Weight(0.4f)
                );