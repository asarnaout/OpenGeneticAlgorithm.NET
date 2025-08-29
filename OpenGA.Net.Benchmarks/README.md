![OpenGA.Net logo](../assets/openga-logo.svg)

# 🧬 OpenGA.Net Benchmarks

This directory contains comprehensive benchmarks for the OpenGA.Net genetic algorithm library, featuring implementations of three classic NP-hard optimization problems: Traveling Salesman Problem (TSP), Knapsack Problem, and Bin Packing Problem.

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 or later
- OpenGA.Net library (included as project reference)

### Running Benchmarks

```bash
# Quick performance benchmarks (recommended for first run)
dotnet run -- --simple

# Detailed solution quality analysis
dotnet run -- --analysis

# Comprehensive BenchmarkDotNet performance suite
dotnet run

# From repository root
dotnet run --project OpenGA.Net.Benchmarks -- --simple
```

## 📁 Project Structure

```
OpenGA.Net.Benchmarks/
├── Problems/                           # Problem implementations
│   ├── TravelingSalesmanProblem.cs    # TSP chromosome & generators
│   ├── KnapsackProblem.cs              # Knapsack chromosome & generators
│   └── BinPackingProblem.cs            # Bin packing chromosome & generators
├── BenchmarkSuite.cs                   # BenchmarkDotNet test suite
├── SimpleBenchmark.cs                  # Quick performance tests
├── Program.cs                          # Main entry point
├── BENCHMARK_RESULTS.md                # Detailed results & analysis
└── README.md                           # This file
```

## 🔬 Benchmark Problems

### 🗺️ Traveling Salesman Problem (TSP)
**Objective**: Find the shortest route visiting all cities exactly once and returning to start.

**Features**:
- Euclidean distance calculation between cities
- 2-opt mutation for local search improvement
- Genetic repair ensuring valid permutations
- Test instances: 30 and 50 cities

**Chromosome Design**:
```csharp
public class TspChromosome : Chromosome<int>
{
    // Genes represent city sequence (permutation)
    // Fitness = 1 / (1 + total_distance)
    // Mutation: 2-opt segment reversal
    // Repair: Fix duplicate cities
}
```

### 🎒 Knapsack Problem
**Objective**: Maximize value of items selected while staying within weight capacity constraints.

**Features**:
- Binary representation (item included/excluded)
- Penalty-based fitness for constraint violations
- Greedy repair mechanism for invalid solutions
- Test instances: 50 and 100 items

**Chromosome Design**:
```csharp
public class KnapsackChromosome : Chromosome<bool>
{
    // Genes[i] = true if item i is included
    // Fitness = total_value - penalty_for_overweight
    // Mutation: Random bit flipping
    // Repair: Greedy removal of low-value items
}
```

### 📦 Bin Packing Problem
**Objective**: Pack items into the minimum number of fixed-capacity bins.

**Features**:
- Multi-objective fitness (minimize bins + maximize utilization)
- First-Fit Decreasing and Best-Fit initialization
- Sophisticated mutation with local improvement
- Test instances: 50 and 100 items

**Chromosome Design**:
```csharp
public class BinPackingChromosome : Chromosome<int>
{
    // Genes[i] = bin assignment for item i
    // Fitness = bin_efficiency * 0.7 + utilization * 0.3
    // Mutation: Reassignment, swapping, local improvement
    // Repair: First-fit for overloaded bins
}
```

## ⚙️ Configuration Examples

### High-Performance Configuration
```csharp
var result = await OpenGARunner<int>
    .Initialize(population)
    .WithRandomSeed(42)
    .MutationRate(0.1f)
    .ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
    .Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
    .SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
    .Termination(t => t.MaximumEpochs(200))
    .RunToCompletionAsync();
```

### Exploration-Focused Configuration
```csharp
var result = await OpenGARunner<int>
    .Initialize(population)
    .WithRandomSeed(42)
    .MutationRate(0.2f)
    .ParentSelection(c => c.RegisterSingle(s => s.RouletteWheel()))
    .Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
    .SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
    .Termination(t => t.MaximumEpochs(500))
    .RunToCompletionAsync();
```

## 📊 Benchmark Results Summary

| Problem | Instance | Time (500 gen) | Best Solution | Quality Grade |
|---------|----------|----------------|---------------|---------------|
| TSP | 30 cities | 1,111ms | 4,689.72 distance (68.4% vs random) | A+ |
| TSP | 50 cities | 1,149ms | 7,842.88 distance | A |
| Knapsack | 50 items | 1,075ms | 1,027.80 value (99.94% efficiency) | A+ |
| Knapsack | 100 items | 1,130ms | 2,286.87 value (99.93% efficiency) | A+ |
| Bin Packing | 50 items | 1,415ms | 19/18 bins (94.7%) | A |
| Bin Packing | 100 items | 540ms | 36/36 bins (100%) | A+ |

## 🛠️ Extending the Benchmarks

### Adding New Problems

1. **Create Chromosome Class**:
```csharp
public class MyProblemChromosome : Chromosome<T>
{
    public override async Task<double> CalculateFitnessAsync() { /* ... */ }
    public override async Task MutateAsync(Random random) { /* ... */ }
    public override async Task<Chromosome<T>> DeepCopyAsync() { /* ... */ }
    public override async Task GeneticRepairAsync() { /* ... */ }
}
```

2. **Create Instance Generator**:
```csharp
public static class MyProblemInstanceGenerator
{
    public static MyProblemChromosome[] GenerateInitialPopulation(int size, ...)
    {
        // Generate diverse initial solutions
    }
}
```

3. **Add Benchmark Method**:
```csharp
[Benchmark(Description = "My Problem Description")]
public async Task<MyProblemChromosome> MyProblemBenchmark()
{
    // Configure and run genetic algorithm
}
```

### Customizing Benchmark Parameters

Edit these constants in `BenchmarkSuite.cs`:
```csharp
private const int POPULATION_SIZE = 100;    // Population size
private const int MAX_GENERATIONS = 500;    // Generation limit
private const int BENCHMARK_SEED = 42;      // Random seed for reproducibility
```

## 📈 Performance Analysis

### Interpreting Results

- **Execution Time**: Lower is better, but consider solution quality trade-offs
- **Fitness Values**: Problem-specific, higher generally indicates better solutions
- **Convergence Rate**: How quickly the algorithm finds good solutions
- **Solution Validity**: Whether constraints are properly satisfied

### Strategy Comparison

The benchmarks test multiple strategy combinations:
- **Parent Selection**: Tournament, RouletteWheel, Elitist
- **Crossover**: OnePoint, KPoint, Uniform
- **Survivor Selection**: Elitist, Generational, Tournament

### Hardware Considerations

Performance will vary based on:
- CPU speed and architecture
- Available memory
- .NET runtime version
- Concurrent system load

## 🔍 Troubleshooting

### Common Issues

**Build Errors**:
```bash
dotnet restore
dotnet clean
dotnet build
```

**Slow Performance**:
- Reduce population size or generations for testing
- Check for inefficient fitness functions
- Profile memory usage

**Poor Solution Quality**:
- Increase mutation rate for exploration
- Try different operator combinations
- Verify chromosome repair logic

### Debug Mode

Add debug output to chromosome classes:
```csharp
public override async Task<double> CalculateFitnessAsync()
{
    var fitness = CalculateFitness();
    Console.WriteLine($"Fitness: {fitness}");
    return await Task.FromResult(fitness);
}
```

## 📚 References

- **Genetic Algorithms**: Holland, J.H. (1992). "Adaptation in Natural and Artificial Systems"
- **TSP**: Applegate, D. et al. (2007). "The Traveling Salesman Problem: A Computational Study"
- **Bin Packing**: Johnson, D.S. (1973). "Near-optimal bin packing algorithms"

## 🤝 Contributing

We welcome contributions to improve the benchmark suite:

1. **New Problem Implementations**: Add more classic optimization problems
2. **Performance Optimizations**: Improve chromosome implementations
3. **Analysis Tools**: Enhanced visualization and reporting
4. **Documentation**: Better examples and explanations

### Contribution Guidelines

- Follow existing code style and patterns
- Include comprehensive tests for new problems
- Document performance characteristics
- Provide example usage

---

For detailed results and analysis, see **[BENCHMARK_RESULTS.md](BENCHMARK_RESULTS.md)**
