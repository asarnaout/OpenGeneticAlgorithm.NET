namespace OpenGA.Net.Tests;

public class DummyChromosome(List<int> genes) : Chromosome<int>(genes)
{
    public override double CalculateFitness()
    {
        return Genes.Average();
    }

    public override Chromosome<int> DeepCopy()
    {
        var list = new List<int>();

        foreach (var item in Genes)
        {
            list.Add(item);
        }

        return new DummyChromosome(list);
    }

    public override void Mutate()
    {
        var random = new Random();

        Genes[random.Next(0, Genes.Count)]++;
    }

    public override void GeneticRepair()
    {
        for(var i = 0; i < Genes.Count; i++)
        {
            if(Genes[i] % 2 != 0)
            {
                Genes[i]++;
            }
        }
    }

}
