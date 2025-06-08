namespace OpenGA.Net.EliminationMechanisms;

public abstract class BaseEliminationMechanism<T>
{
    protected internal abstract IEnumerable<Chromosome<T>> ApplyReplacementStrategy(IEnumerable<Chromosome<T>> population, Random random);
}
