namespace OpenGA.Net.Tests;

public class DummyChromosome(List<int> genes) : Chromosome<int>(genes)
{
    public override Task<double> CalculateFitnessAsync()
    {
        return Task.FromResult(Genes.Average());
    }

    public override Task<Chromosome<int>> DeepCopyAsync()
    {
        var list = new List<int>();

        foreach (var item in Genes)
        {
            list.Add(item);
        }

        return Task.FromResult<Chromosome<int>>(new DummyChromosome(list));
    }

    public override Task MutateAsync(Random random)
    {
        Genes[random.Next(0, Genes.Count)]++;
        
        return Task.CompletedTask;
    }

    public override Task GeneticRepairAsync()
    {
        for(var i = 0; i < Genes.Count; i++)
        {
            if(Genes[i] % 2 != 0)
            {
                Genes[i]++;
            }
        }
        
        return Task.CompletedTask;
    }

}
