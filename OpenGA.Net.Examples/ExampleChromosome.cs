
namespace OpenGA.Net.Examples;

public class ExampleChromosome : Chromosome<float>
{
    public ExampleChromosome(float[] genes) : base(genes)
    {
    }

    public override double CalculateFitness()
    {
        return Genes.Sum();
    }

    public override Task MutateAsync()
    {
        throw new NotImplementedException();
    }

    public override Task PostCrossoverAsync()
    {
        throw new NotImplementedException();
    }
}
