namespace OpenGA.Net.Tests;

public class DummyChromosome(int[] genes) : Chromosome<int>(genes)
{
    public override double CalculateFitness()
    {
        return Genes.Average();
    }

    public override Task MutateAsync()
    {
        var random = new Random();

        Genes[random.Next(0, Genes.Length)]++;

        return Task.CompletedTask;
    }

    public override Task PostCrossoverAsync()
    {
        for(var i = 0; i < Genes.Length; i++)
        {
            if(Genes[i] % 2 != 0)
            {
                Genes[i]++;
            }
        }

        return Task.CompletedTask;
    }
}
