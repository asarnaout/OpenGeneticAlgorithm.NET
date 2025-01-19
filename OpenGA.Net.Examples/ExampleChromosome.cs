
namespace OpenGA.Net.Examples;

public class ExampleChromosome(float[] genes) : Chromosome<float>(genes)
{
    public override double CalculateFitness()
    {
        return Genes.Sum();
    }

    public override void Mutate()
    {
        throw new NotImplementedException();
    }

    public override void PostCrossover()
    {
        throw new NotImplementedException();
    }

    public override Chromosome<float> DeepCopy()
    {
        var copiedGenes = new float[Genes.Length];

        Array.Copy(Genes, copiedGenes, Genes.Length);

        return new ExampleChromosome(copiedGenes);
    }
}
