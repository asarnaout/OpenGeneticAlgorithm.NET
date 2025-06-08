namespace OpenGA.Net.EliminationMechanisms;

internal class ElitistEliminationMechanism<T> : BaseEliminationMechanism<T>
{
    protected internal override IEnumerable<Chromosome<T>> ApplyReplacementStrategy(IEnumerable<Chromosome<T>> population, Random random)
    {
        return null;
    }
}
