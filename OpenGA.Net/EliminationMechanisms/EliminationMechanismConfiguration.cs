namespace OpenGA.Net.EliminationMechanisms;

public class EliminationMechanismConfiguration<T>
{
    internal BaseEliminationMechanism<T> EliminationMechanism = default!;

    /// <summary>
    /// 
    /// </summary>
    public BaseEliminationMechanism<T> Random()
    {
        var result = new RandomEliminationMechanism<T>();
        EliminationMechanism = result;
        return result;
    }

    /// <summary>
    /// 
    /// </summary>
    public BaseEliminationMechanism<T> Elitism()
    {
        var result = new ElitistEliminationMechanism<T>();
        EliminationMechanism = result;
        return result;
    }
}
