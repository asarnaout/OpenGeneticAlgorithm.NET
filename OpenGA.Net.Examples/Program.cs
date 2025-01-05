using OpenGA.Net;
using OpenGA.Net.Examples;

//WIP
var runner = OpenGARunner<float>
                .Init([new ExampleChromosome([])])
                .ApplyReproductionSelectors(c => c.ApplyRandomReproductionSelector().And(s => s.ApplyRankSelectionReproductionSelector().And(t => t.ApplyElitistReproductionSelector())));

