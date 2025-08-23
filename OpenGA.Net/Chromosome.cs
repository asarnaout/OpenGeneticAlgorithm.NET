using System.Linq.Expressions;

namespace OpenGA.Net;

/// <summary>
/// A Chromosome represents a potential solution to the given optimization problem. Chromosomes are made of 
/// <see cref="Genes">Genes</see> which represent components of the solution. For example, in the Traveling
/// Salesman Problem, a Gene could be an integer representing the city while the chromosome would hold an array
/// of the latter genes representing the sequence of cities to be traversed in order.
/// </summary>
public abstract class Chromosome<T>(IList<T> genes) : IEquatable<Chromosome<T>>
{
    public IList<T> Genes { get; internal set; } = genes;

    internal Guid InternalIdentifier { get; } = Guid.NewGuid();

    /// <summary>
    /// The age of the chromosome, representing how many epochs/generations it has survived.
    /// This starts at 0 when the chromosome is created and increments each generation it survives.
    /// </summary>
    public int Age { get; internal set; } = 0;

    public bool Equals(Chromosome<T>? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other) || InternalIdentifier == other.InternalIdentifier)
        {
            return true;
        }

        return false;
    }

    public static bool operator ==(Chromosome<T> obj1, Chromosome<T> obj2) => obj1.Equals(obj2);

    public static bool operator !=(Chromosome<T> obj1, Chromosome<T> obj2) => !obj1.Equals(obj2);

    /// <summary>
    /// The fitness function evaluates the favorability of the chromosome as a potential solution to the optimization
    /// problem. Override this method to return a value reflecting this favorability.
    /// </summary>
    public abstract double CalculateFitness();

    /// <summary>
    /// Override this method to provide a custom Mutation implementation to the Chromosome. An example of a mutation
    /// is to randomly delete a member of the <see cref="Genes">Genes</see> array.
    /// </summary>
    public abstract void Mutate();

    /// <summary>
    /// Since Chromosomes partake in crossover operations, we will need to ensure that we can create deep copies of
    /// the mating chromosome(s). Override the DeepCopy method to provide such an implementation.
    /// </summary>
    public abstract Chromosome<T> DeepCopy();

    /// <summary>
    /// Increments the age of the chromosome by 1. This should be called at the end of each generation
    /// for chromosomes that survive to the next generation.
    /// </summary>
    internal void IncrementAge()
    {
        Age++;
    }

    /// <summary>
    /// Resets the age of the chromosome to 0. This should be called for newly created offspring chromosomes.
    /// </summary>
    internal void ResetAge()
    {
        Age = 0;
    }

    /// <summary>
    /// This method is No-Op by default and will be called on all chromosomes (after crossover and mutation take place) 
    /// to ensure that no illegal chromosomes exist within the population. Override to introduce a custom operation to 
    /// run on a chromosome to ensure that it conforms to any  rules that might dictate what a valid solution is. 
    /// An example of such an operation is to eliminate any duplicates (if necessary) from the <see cref="Genes">Genes</see> 
    /// array in the case of the TSP.
    /// </summary>
    public virtual void GeneticRepair() => Expression.Empty();

    public override bool Equals(object? obj)
    {
        return Equals(obj as Chromosome<T>);
    }

    public override int GetHashCode()
    {
        return InternalIdentifier.GetHashCode();
    }
}