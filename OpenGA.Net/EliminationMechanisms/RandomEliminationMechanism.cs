namespace OpenGA.Net.EliminationMechanisms;

public class RandomEliminationMechanism<T> : BaseEliminationMechanism<T>
{
    protected internal override IEnumerable<Chromosome<T>> ApplyReplacementStrategy(IEnumerable<Chromosome<T>> population, Random random)
    {
        return null;
    }
}
