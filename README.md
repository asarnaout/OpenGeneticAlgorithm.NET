![OpenGA.Net logo](assets/openga-logo.svg)

# 🧬 OpenGeneticAlgorithm.NET

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/OpenGA.Net.svg)](https://www.nuget.org/packages/OpenGA.Net/)
[![Downloads](https://img.shields.io/nuget/dt/OpenGA.Net.svg)](https://www.nuget.org/packages/OpenGA.Net/)

**The most intuitive, powerful, and extensible genetic algorithm framework for .NET**

OpenGA.Net is a high-performance, type-safe genetic algorithm library that makes evolutionary computation accessible to everyone. Whether you're solving complex optimization problems, researching machine learning, or just getting started with genetic algorithms, OpenGA.Net provides the tools you need with an elegant, fluent API.

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package OpenGA.Net
```

### Your First Genetic Algorithm

A **Chromosome** in OpenGA.Net represents a potential solution to your optimization problem. It contains **genes** (the solution components) and defines how to evaluate, modify, and repair solutions.

**Step 1:** Create your chromosome by inheriting from the `Chromosome<T>` abstract class, the example below demonstrates how a Chromosome representing a potential solution to the [traveling salesman problem](https://en.wikipedia.org/wiki/Travelling_salesman_problem) could look like:

```csharp
using OpenGA.Net;

// A chromosome for the Traveling Salesman Problem. Each chromosome represents a route through cities (genes = city sequence)
public class TspChromosome(IList<int> cities, double[,] distanceMatrix) : Chromosome<int>(cities)
{
    // Calculate how "good" this route is (shorter distance = higher fitness)
    public override async Task<double> CalculateFitnessAsync()
    {
        double totalDistance = CalculateTotalDistance();
        
        return 1.0 / (1.0 + totalDistance);
    }
    
    // Randomly swap two cities in the route
    public override async Task MutateAsync(Random random)
    {
        int index1 = random.Next(Genes.Count);
        int index2 = random.Next(Genes.Count);
        (Genes[index1], Genes[index2]) = (Genes[index2], Genes[index1]);
    }
    
    public override async Task<Chromosome<int>> DeepCopyAsync()
    {
        return new TspChromosome(new List<int>(Genes), distanceMatrix);
    }
    
    public override async Task GeneticRepairAsync()
    {
        // Ensure each city appears exactly once (fix any duplicates)
    }
}
```

**Step 2:** Create your initial population:

```csharp
// Generate initial population of random routes. The initial population represents a set of random solutions to the optimization problem.
var initialPopulation = new TspChromosome[100];
for (int i = 0; i < 100; i++)
{
    var cities = Enumerable.Range(0, numberOfCities).OrderBy(x => _random.Next()).ToList();
    initialPopulation[i] = new TspChromosome(cities, _distanceMatrix);
}
```

**Step 3:** Configure and run your genetic algorithm:

```csharp
// Configure and run your genetic algorithm
var bestSolution = await OpenGARunner<int>
    .Initialize(initialPopulation)
    .RunToCompletionAsync();

// Get your optimized result
Console.WriteLine($"Best route: {string.Join(" → ", bestSolution.Genes)}");
```

That's it! 🎉 You just solved an optimization problem with a few lines of code.

---

## 🏗️ Architecture Overview

OpenGA.Net is built around four core concepts that work together seamlessly:

### 🧬 **Chromosomes**
Your solution candidates. Strongly typed and extensible. Each `Chromosome<T>` subclass defines how a solution is represented and evaluated. Implement:

- CalculateFitnessAsync() — return a higher-is-better score for the current Genes
- MutateAsync() — randomly perturb Genes to explore the search space
- DeepCopyAsync() — produce a full copy used during crossover
- GeneticRepairAsync() (optional) — fix invalid states after crossover/mutation

Tip: See the TSP chromosome in the Quick Start above for a concrete example. The pattern is the same for any problem: choose a gene representation, define a fitness function, add a simple mutation, and optionally repair to enforce constraints.

### 🎯 **Parent Selection Strategies**
Choose the chromosomes that will participate in mating/crossover:

| Strategy | When to Use | Problem Characteristics | Population Size | Fitness Landscape |
|----------|-------------|------------------------|-----------------|-------------------|
| **Tournament** *(Default)* | Default choice for most problems | Balanced exploration/exploitation needed | Any size | Noisy or multimodal landscapes |
| **Elitist** | Need guaranteed convergence | High-quality solutions must be preserved | Medium to large (50+) | Clear fitness hierarchy |
| **Roulette Wheel** | Fitness-proportionate diversity | Wide fitness range, avoid premature convergence | Large (100+) | Smooth, unimodal landscapes |
| **Boltzmann** | Dynamic selection pressure | Need cooling schedule control | Medium to large | Complex, deceptive landscapes |
| **Rank Selection** | Prevent fitness scaling issues | Similar fitness values across population | Any size | Flat or highly scaled fitness |
| **Random** | Maximum diversity exploration | Early stages or highly exploratory search | Any size | Unknown or chaotic landscapes |

### 🧬 **Crossover Strategies**
Create offspring by combining parent chromosomes:

| Strategy | When to Use | Gene Representation | Problem Type | Preservation Needs |
|----------|-------------|-------------------|--------------|-------------------|
| **One-Point** *(Default)* | Fast, simple problems | Order doesn't matter critically | Optimization with independent variables | Preserve some gene clusters |
| **K-Point** | Moderate complexity balance | Mixed dependencies between genes | Multi-dimensional optimization | Control disruption level |
| **Uniform** | Maximum genetic diversity | Independent genes | Exploratory search, avoid local optima | No specific gene clustering |
| **Custom** | Domain-specific requirements | Complex constraints or structures | Specialized problem domains | Domain-specific validation |

### 🔄 **Survivor Selection Strategies**
Manage population evolution over generations:

| Strategy | When to Use | Convergence Speed | Population Diversity | Resource Constraints |
|----------|-------------|-------------------|---------------------|---------------------|
| **Elitist** *(Default)* | Most optimization problems | Fast convergence | Moderate diversity loss | Low computational overhead |
| **Generational** | Exploration-heavy search | Slower, thorough exploration | High diversity maintained | Higher memory usage |
| **Tournament** | Balanced performance | Moderate convergence | Good diversity balance | Moderate computational cost |
| **Age-based** | Long-running evolutionary systems | Very slow, stable | Excellent long-term diversity | Requires age tracking |
| **Boltzmann** | Temperature-controlled evolution | Adaptive convergence | Dynamic diversity control | Higher computational complexity |
| **Random Elimination** | Maintain population diversity | Slowest convergence | Maximum diversity | Minimal computational overhead |


### 🏁 **Termination Strategies**
Control when the genetic algorithm stops evolving:

| Strategy | When to Use | Stopping Condition | Best For | Predictability |
|----------|-------------|-------------------|----------|----------------|
| **Maximum Epochs** *(Default)* | Known iteration limits | Fixed number of generations | Time-constrained scenarios | Highly predictable runtime |
| **Maximum Duration** | Real-time applications | Maximum execution duration | Production systems | Predictable time bounds |
| **Target Standard Deviation** | Diversity monitoring | Low population diversity | Avoiding premature convergence | Adaptive stopping |
| **Target Fitness** | Goal-oriented optimization | Specific fitness threshold reached | Known optimal solution value | Adaptive based on performance |

### 💡 **Strategy Selection Examples**

```csharp
// High-performance optimization (fast convergence needed)
.ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
.Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
.SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
.Termination(t => t.MaximumEpochs(100))

// Exploratory search (avoiding local optima)
.ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
.Crossover(c => c.RegisterSingle(s => s.UniformCrossover()))
.SurvivorSelection(r => r.RegisterSingle(s => s.Generational()))
.Termination(t => t.TargetStandardDeviation(stdDev: 0.001))

// Production system (time-constrained)
.ParentSelection(c => c.RegisterSingle(s => s.Tournament()))
.Crossover(c => c.RegisterSingle(s => s.OnePointCrossover()))
.SurvivorSelection(r => r.RegisterSingle(s => s.Elitist()))
.Termination(t => t.MaximumDuration(TimeSpan.FromMinutes(5)))

// Quality-focused research (target fitness termination)
.ParentSelection(c => c.RegisterSingle(s => s.RouletteWheel()))
.Crossover(c => c.RegisterSingle(s => s.KPointCrossover(3)))
.SurvivorSelection(r => r.RegisterSingle(s => s.Elitist(0.2f)))
.Termination(t => t.TargetFitness(0.95).TargetStandardDeviation(stdDev: 0.001, window: 10))
```

---

## 🎯 Use Cases

### 🏭 **Optimization Problems**
- **Scheduling**: Job shop, vehicle routing, resource allocation
- **Engineering Design**: Antenna design, circuit optimization, structural engineering
- **Financial**: Portfolio optimization, trading strategies, risk management

### 🧠 **Machine Learning**
- **Neural Architecture Search**: Automatically design neural networks
- **Hyperparameter Tuning**: Optimize ML model parameters
- **Feature Selection**: Find optimal feature subsets

### 🎮 **Game Development**
- **AI Behavior**: Evolve intelligent game agents
- **Level Generation**: Create procedural game content
- **Balancing**: Optimize game mechanics and difficulty curves

### 🔬 **Research & Academia**
- **Evolutionary Computation**: Research platform for GA variants
- **Multi-objective Optimization**: Pareto-optimal solutions
- **Algorithm Comparison**: Benchmark different evolutionary strategies

---

## ⚙️ **Advanced Configuration**

For sophisticated optimization scenarios, OpenGA.Net supports advanced multi-strategy configurations with **Adaptive Pursuit** - an intelligent operator selection policy that learns which strategies perform best over time and automatically adjusts selection probabilities.

### 🧠 **Adaptive Pursuit Algorithm**

Adaptive Pursuit continuously monitors the performance of each configured strategy and dynamically adapts their selection probabilities based on:
- **Fitness improvement** from applied operators
- **Population diversity** maintenance 
- **Recent performance trends** with temporal weighting
- **Exploration vs exploitation** balance

This creates a self-optimizing genetic algorithm that automatically discovers the best operator combinations for your specific problem.

### � **Multi-Strategy Configuration**

```csharp
var result = await OpenGARunner<int>
    .Initialize(initialPopulation, minPopulationPercentage: 0.4f, maxPopulationPercentage: 2.5f)
    .WithRandomSeed(42)
    .MutationRate(0.15f)

    // Multi-strategy parent selection with adaptive learning
    .ParentSelection(p => p.RegisterMulti(m => m
        .Tournament(stochasticTournament: true)
        .RouletteWheel()
        .Rank(customWeight: 0.2f)
        .Boltzmann(temperatureDecayRate: 0.05, initialTemperature: 1.0)
        .WithPolicy(policy => policy.AdaptivePursuit(
            learningRate: 0.12,
            minimumProbability: 0.05,
            rewardWindowSize: 15
        ))
    ))

    // Round Robin crossover for exploration
    .Crossover(c => c.RegisterMulti(s => s
        .KPointCrossover(3)
        .UniformCrossover()
        .WithPolicy(policy => policy.RoundRobin())
    ).WithCrossoverRate(0.85f))

    // Multi-strategy survivor selection for dynamic population management.
    // Each strategy is assigned a custom probability weight.
    .SurvivorSelection(s => s.RegisterMulti(m => m
        .Elitist(elitePercentage: 0.12f, customWeight: 0.6f)
        .Tournament(tournamentSize: 4, stochasticTournament: true, customWeight: 0.3f)
        .AgeBased(customWeight: 0.1f)
        .WithPolicy(policy => policy.CustomWeights())
    ).OverrideOffspringGenerationRate(1.3f))
    
    // Multi-condition termination
    .Termination(t => t
        .MaximumEpochs(750)
        .MaximumDuration(TimeSpan.FromMinutes(10))
        .TargetFitness(0.98)
        .TargetStandardDeviation(0.001, window: 20))
    
    .RunToCompletionAsync();
```

---

## 🛠️ Extensibility

Create your own strategies by inheriting from base classes:

```csharp
// Custom crossover strategy
public class MyCustomCrossover<T> : BaseCrossoverStrategy<T>
{
    protected internal override async Task<IEnumerable<Chromosome<T>>> CrossoverAsync(
        Couple<T> couple, Random random)
    {
        // Your custom crossover logic
    }
}

// Custom survivor selection strategy  
public class MySurvivorSelectionStrategy<T> : BaseSurvivorSelectionStrategy<T>
{
    internal override float RecommendedOffspringGenerationRate => 0.5f;
    
    protected internal override async Task<IEnumerable<Chromosome<T>>> SelectChromosomesForEliminationAsync(
        Chromosome<T>[] population, Chromosome<T>[] offspring, Random random, int currentEpoch = 0)
    {
        // Your custom survivor selection logic
    }
}

// Custom reproduction selector
public class MyParentSelector<T> : BaseParentSelectorStrategy<T>
{
    protected internal override async Task<IEnumerable<Couple<T>>> SelectMatingPairsAsync(
        Chromosome<T>[] population, Random random, int minimumNumberOfCouples)
    {
        // Your custom parent selection logic
    }
}

// Custom termination strategy
public class MyTerminationStrategy<T> : BaseTerminationStrategy<T>
{   
    public override bool Terminate(GeneticAlgorithmState state)
    {
        // Your custom logic to control when the GA should terminate
    }
}

// Using your custom strategies
var result = await OpenGARunner<MyGeneType>
    .Initialize(initialPopulation)
    .ParentSelection(c => c.RegisterSingle(s => s.Custom(new MyParentSelector<MyGeneType>())))
    .Crossover(c => c.RegisterSingle(s => s.CustomCrossover(new MyCustomCrossover<MyGeneType>())))
    .SurvivorSelection(r => r.RegisterSingle(s => s.Custom(new MySurvivorSelectionStrategy<MyGeneType>())))
    .Termination(t => t.Custom(new MyTerminationStrategy<MyGeneType>(50)))
    .RunToCompletionAsync();
```

---

## 📊 Performance & Benchmarks

OpenGA.Net has been rigorously tested on classic optimization problems to demonstrate its performance and solution quality. Our comprehensive benchmark suite includes:

### 🔬 **Benchmark Problems**
- **🗺️ Traveling Salesman Problem (TSP)**: 30 and 50 city instances
- **🎒 Knapsack Problem**: 50 and 100 item optimization
- **📦 Bin Packing Problem**: 50 and 100 item optimization

### ⚡ **Performance Highlights**
- **Execution Speed**: 601ms-2,321ms for 500 generations on complex problems
- **Scalability**: Linear scaling with population size and problem complexity
- **Solution Quality**: Consistently achieves near-optimal results within 1-2% of known bounds

### 📈 **Key Results**

| Problem | Instance | Time (500 gen) | Best Result |
|---------|----------|----------------|-------------|
| TSP | 30 cities | 1,853ms | 71.1% better than random tour |
| TSP | 50 cities | 1,893ms | 69.7% better than random tour |
| Knapsack | 50 items | 1,863ms | 1,027.8 value (99.94% efficiency) |
| Knapsack | 100 items | 601ms | 2,286.2 value (99.90% efficiency) |
| Bin Packing | 50 items | 2,321ms | 19 bins (vs 18 optimal) |
| Bin Packing | 100 items | 993ms | 36 bins (optimal!) |

**Result Interpretation:**
- **TSP**: % improvement over random tours / absolute distance (showing significant optimization capability)
- **Knapsack**: Total value achieved and efficiency vs upper bound (nearly optimal solutions)
- **Bin Packing**: Bins used (achieving optimal or near-optimal solutions)

### 🏃 **Run Benchmarks Yourself**

```bash
cd OpenGA.Net.Benchmarks

# Quick performance tests
dotnet run --simple

# Detailed solution quality analysis  
dotnet run --analysis

# Comprehensive BenchmarkDotNet suite
dotnet run
```

📋 **[View Complete Benchmark Results](OpenGA.Net.Benchmarks/BENCHMARK_RESULTS.md)** - Detailed methodology, configurations, and analysis

---

## 🤝 Contributing

We welcome contributions! OpenGA.Net is built by the community, for the community.

### 🐛 **Found a Bug?**
[Open an issue](https://github.com/asarnaout/OpenGeneticAlgorithm.Net/issues) with:
- Clear reproduction steps
- Expected vs actual behavior
- Environment details

### 🔧 **Want to Code?**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes with tests
4. Submit a pull request

---

## 📄 License

OpenGA.Net is released under the **MIT License**. See [LICENSE.md](LICENSE.md) for details.

```
Copyright (c) 2024 Ahmed Sarnaout

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
```

---