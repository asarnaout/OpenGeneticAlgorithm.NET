namespace OpenGA.Net;

internal enum ReplacementStrategy
{
    None = 0,
    
    /// <summary>
    /// Random individuals are eliminated each epoch/generation.
    /// </summary>
    Random = 1,

    /// <summary>
    /// Every epoch/generation, the entire population will be replaced with its offspring.
    /// </summary>
    Generational = 2,

    /// <summary>
    /// The more fit an individual chromosome is, the more likely it will survive the current epoch/generation.
    /// </summary>
    RouletteWheel = 4,

    /// <summary>
    /// The longer a chromosome survives, the more likely it will be eliminated.
    /// </summary>
    AgeBased = 8,

    /// <summary>
    /// Elite chromosomes are guaranteed to survive the current epoch/generation.
    /// </summary>
    Elitism = 16,

    Tournament = 32
}
