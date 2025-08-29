using OpenGA.Net;

namespace OpenGA.Net.Benchmarks.Problems;

/// <summary>
/// N-Queens Problem implementation for benchmarking genetic algorithms.
/// The N-Queens problem is to place N queens on an N×N chessboard such that no two queens
/// can attack each other (no two queens share the same row, column, or diagonal).
/// </summary>
public class NQueensChromosome : Chromosome<int>
{
    private readonly int _boardSize;
    private readonly Random _random = new();

    public NQueensChromosome(IList<int> queenPositions, int boardSize) : base(queenPositions)
    {
        _boardSize = boardSize;
    }

    /// <summary>
    /// Calculate fitness based on the number of non-attacking queen pairs.
    /// Perfect solution has fitness = 1.0, where no queens attack each other.
    /// </summary>
    public override async Task<double> CalculateFitnessAsync()
    {
        int conflicts = CountConflicts();
        int maxPossibleConflicts = _boardSize * (_boardSize - 1) / 2; // n*(n-1)/2 possible pairs
        
        // Fitness is the percentage of non-conflicting queen pairs
        double fitness = 1.0 - (double)conflicts / maxPossibleConflicts;
        return await Task.FromResult(Math.Max(0, fitness));
    }

    /// <summary>
    /// Mutation by swapping two random queen positions.
    /// This maintains the constraint that each row has exactly one queen.
    /// </summary>
    public override async Task MutateAsync(Random random)
    {
        if (Genes.Count < 2) return;

        int pos1 = random.Next(Genes.Count);
        int pos2 = random.Next(Genes.Count);
        
        // Swap the column positions of two queens
        (Genes[pos1], Genes[pos2]) = (Genes[pos2], Genes[pos1]);
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Create a deep copy of this chromosome.
    /// </summary>
    public override async Task<Chromosome<int>> DeepCopyAsync()
    {
        return await Task.FromResult(new NQueensChromosome(new List<int>(Genes), _boardSize));
    }

    /// <summary>
    /// Repair invalid gene values to ensure they are within board boundaries.
    /// </summary>
    public override async Task GeneticRepairAsync()
    {
        for (int i = 0; i < Genes.Count; i++)
        {
            // Ensure column position is within board boundaries
            if (Genes[i] < 0 || Genes[i] >= _boardSize)
            {
                Genes[i] = _random.Next(_boardSize);
            }
        }
        
        await Task.CompletedTask;
    }

    /// <summary>
    /// Count the number of conflicts (attacking queen pairs) in the current configuration.
    /// </summary>
    private int CountConflicts()
    {
        int conflicts = 0;
        
        for (int i = 0; i < Genes.Count; i++)
        {
            for (int j = i + 1; j < Genes.Count; j++)
            {
                // Check if queens at row i and row j are attacking each other
                if (Genes[i] == Genes[j] || // Same column
                    Math.Abs(Genes[i] - Genes[j]) == Math.Abs(i - j)) // Same diagonal
                {
                    conflicts++;
                }
            }
        }
        
        return conflicts;
    }

    /// <summary>
    /// Check if this is a valid solution (no conflicts).
    /// </summary>
    public bool IsSolution() => CountConflicts() == 0;

    /// <summary>
    /// Get the number of conflicts for reporting purposes.
    /// </summary>
    public int GetConflicts() => CountConflicts();

    /// <summary>
    /// Get a string representation of the board for visualization.
    /// </summary>
    public string GetBoardRepresentation()
    {
        var board = new char[_boardSize, _boardSize];
        
        // Initialize board with dots
        for (int i = 0; i < _boardSize; i++)
        {
            for (int j = 0; j < _boardSize; j++)
            {
                board[i, j] = '.';
            }
        }
        
        // Place queens
        for (int row = 0; row < Genes.Count && row < _boardSize; row++)
        {
            if (Genes[row] >= 0 && Genes[row] < _boardSize)
            {
                board[row, Genes[row]] = 'Q';
            }
        }
        
        // Convert to string
        var result = "";
        for (int i = 0; i < _boardSize; i++)
        {
            for (int j = 0; j < _boardSize; j++)
            {
                result += board[i, j] + " ";
            }
            result += "\n";
        }
        
        return result;
    }
}

/// <summary>
/// N-Queens instance generator and utilities.
/// </summary>
public static class NQueensInstanceGenerator
{
    /// <summary>
    /// Generate initial population for N-Queens problem.
    /// Each chromosome represents a permutation where index = row and value = column.
    /// </summary>
    public static NQueensChromosome[] GenerateInitialPopulation(int populationSize, int boardSize, int seed = 42)
    {
        var random = new Random(seed);
        var population = new NQueensChromosome[populationSize];
        
        for (int i = 0; i < populationSize; i++)
        {
            // Create a random permutation - this ensures no two queens in same column
            var queenPositions = Enumerable.Range(0, boardSize)
                .OrderBy(x => random.Next())
                .ToList();
            
            population[i] = new NQueensChromosome(queenPositions, boardSize);
        }
        
        return population;
    }

    /// <summary>
    /// Generate a more diverse initial population using different strategies.
    /// </summary>
    public static NQueensChromosome[] GenerateDiverseInitialPopulation(int populationSize, int boardSize, int seed = 42)
    {
        var random = new Random(seed);
        var population = new NQueensChromosome[populationSize];
        
        for (int i = 0; i < populationSize; i++)
        {
            List<int> queenPositions;
            
            if (i % 3 == 0)
            {
                // Strategy 1: Random permutation
                queenPositions = Enumerable.Range(0, boardSize)
                    .OrderBy(x => random.Next())
                    .ToList();
            }
            else if (i % 3 == 1)
            {
                // Strategy 2: Diagonal placement with noise
                queenPositions = new List<int>();
                for (int j = 0; j < boardSize; j++)
                {
                    queenPositions.Add((j + random.Next(3)) % boardSize);
                }
            }
            else
            {
                // Strategy 3: Completely random (may have duplicates, will be handled by repair)
                queenPositions = new List<int>();
                for (int j = 0; j < boardSize; j++)
                {
                    queenPositions.Add(random.Next(boardSize));
                }
            }
            
            population[i] = new NQueensChromosome(queenPositions, boardSize);
        }
        
        return population;
    }

    /// <summary>
    /// Get the theoretical minimum number of conflicts for a given board size.
    /// For most board sizes, the minimum is 0 (perfect solution exists).
    /// </summary>
    public static int GetTheoreticalOptimum(int boardSize)
    {
        // For N-Queens, a solution with 0 conflicts exists for all N except N = 2 and N = 3
        return (boardSize == 2 || boardSize == 3) ? 1 : 0;
    }
}
