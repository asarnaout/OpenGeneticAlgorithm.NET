namespace OpenGA.Net.Tests;

public class DummyChromosome(int[] genes) : Chromosome<int>(genes)
{
    public override double CalculateFitness()
    {
        return Genes.Average();
    }

    public override Chromosome<int> DeepCopy()
    {
        var copiedGenes = new int[Genes.Length];

        Array.Copy(Genes, copiedGenes, Genes.Length);

        return new DummyChromosome(copiedGenes);
    }

    public override void Mutate()
    {
        var random = new Random();

        Genes[random.Next(0, Genes.Length)]++;
    }

    public override void PostCrossover()
    {
        for(var i = 0; i < Genes.Length; i++)
        {
            if(Genes[i] % 2 != 0)
            {
                Genes[i]++;
            }
        }
    }

}
