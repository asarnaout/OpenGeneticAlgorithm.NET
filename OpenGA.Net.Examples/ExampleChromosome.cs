
namespace OpenGA.Net.Examples;

public class ExampleChromosome(IList<float> genes) : Chromosome<float>(genes)
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
        var list = new List<float>();

        foreach (var item in Genes)
        {
            list.Add(item);
        }

        return new ExampleChromosome(list);
    }
}
