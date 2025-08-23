# 🧬 OpenGA.Net

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8.0-purple.svg)](https://dotnet.microsoft.com/download)
[![NuGet](https://img.shields.io/nuget/v/OpenGA.Net.svg)](https://www.nuget.org/packages/OpenGA.Net/)
[![Downloads](https://img.shields.io/nuget/dt/OpenGA.Net.svg)](https://www.nuget.org/packages/OpenGA.Net/)
[![Build Status](https://img.shields.io/github/actions/workflow/status/asarnaout/OpenGeneticAlgorithm.Net/build.yml?branch=main)](https://github.com/asarnaout/OpenGeneticAlgorithm.Net/actions)

**The most intuitive, powerful, and extensible genetic algorithm framework for .NET**

OpenGA.Net is a high-performance, type-safe genetic algorithm library that makes evolutionary computation accessible to everyone. Whether you're solving complex optimization problems, researching machine learning, or just getting started with genetic algorithms, OpenGA.Net provides the tools you need with an elegant, fluent API.

---

## 🚀 Quick Start

### Installation

```bash
dotnet add package OpenGA.Net
```

### Your First Genetic Algorithm

```csharp
using OpenGA.Net;

// Define your problem (Traveling Salesman Problem example)
var cities = new[] { 0, 1, 2, 3, 4 };
var initialPopulation = GenerateRandomRoutes(populationSize: 100, cities);

// Configure and run your genetic algorithm
var solution = OpenGARunner<int>
    .Init(initialPopulation)
    .Epochs(200)
    .MutationRate(0.1f)
    .CrossoverRate(0.8f)
    .ApplyReproductionSelector(c => c.ApplyTournamentReproductionSelector())
    .ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())
    .ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy())
    .RunToCompletion();

// Get your optimized result
var bestRoute = solution.OrderByDescending(chromosome => chromosome.Fitness).First();
Console.WriteLine($"Best route: {string.Join(" → ", bestRoute.Genes)}");
```

That's it! 🎉 You just solved an optimization problem with a few lines of code.

---

## 🏗️ Architecture Overview

OpenGA.Net is built around four core concepts that work together seamlessly:

### 🧬 **Chromosomes**
Your solution candidates. Strongly typed and extensible:

```csharp
public class TspChromosome : Chromosome<int>
{
    public TspChromosome(IList<int> cities) : base(cities) { }
    
    public override double CalculateFitness()
    {
        // Your fitness calculation logic
        return 1.0 / GetTotalDistance();
    }
    
    public override Chromosome<int> DeepCopy()
    {
        return new TspChromosome(new List<int>(Genes));
    }
}
```

### 🎯 **Reproduction Selectors**
Choose the best parents for the next generation:

| Strategy | Use Case |
|----------|----------|
| **Tournament** | General-purpose, good diversity |
| **Elitist** | Preserve best solutions |
| **Roulette Wheel** | Fitness-proportionate selection |
| **Boltzmann** | Temperature-based selection |
| **Rank Selection** | Uniform pressure across population |

### 🔄 **Crossover Strategies**
Create offspring by combining parent chromosomes:

```csharp
// One-point crossover - fast and effective
.ApplyCrossoverStrategy(c => c.ApplyOnePointCrossoverStrategy())

// Uniform crossover - maximum genetic diversity
.ApplyCrossoverStrategy(c => c.ApplyUniformCrossoverStrategy())

// Custom crossover - your own breeding logic
.ApplyCrossoverStrategy(c => c.ApplyCustomCrossoverStrategy(new MyCustomCrossover()))
```

### 🔄 **Replacement Strategies**
Manage population evolution over generations:

| Strategy | Description | Best For |
|----------|-------------|----------|
| **Elitist** | Keep the best, replace the worst | Most problems |
| **Generational** | Replace entire population | Exploration-heavy problems |
| **Tournament** | Compete for survival | Balanced selection pressure |
| **Age-based** | Older chromosomes are more likely to be replaced | Long-term diversity |

---

## 📊 Real-World Examples

### 🗺️ Traveling Salesman Problem

```csharp
// Solve a 50-city TSP in under 1 second
var cities = GenerateRandomCities(50);
var solution = OpenGARunner<int>
    .Init(GenerateInitialPopulation(100, cities))
    .Epochs(500)
    .MutationRate(0.15f)
    .ApplyReproductionSelector(c => c.ApplyTournamentReproductionSelector())
    .ApplyCrossoverStrategy(c => c.ApplyUniformCrossoverStrategy())
    .ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy())
    .RunToCompletion();
```

### 🎛️ Function Optimization

```csharp
// Minimize Rosenbrock function: f(x,y) = (a-x)² + b(y-x²)²
public class RosenbrockChromosome : Chromosome<double>
{
    public override double CalculateFitness()
    {
        var xCoordinate = Genes[0];
        var yCoordinate = Genes[1];
        var result = Math.Pow(1 - xCoordinate, 2) + 100 * Math.Pow(yCoordinate - xCoordinate * xCoordinate, 2);
        return 1.0 / (1.0 + result); // Higher fitness = lower function value
    }
}
```

### 🧠 Neural Network Evolution

```csharp
// Evolve neural network weights
public class NeuralNetworkChromosome : Chromosome<double>
{
    private readonly NeuralNetwork _network;
    
    public override double CalculateFitness()
    {
        _network.SetWeights(Genes);
        return _network.Evaluate(testData);
    }
}
```

---

## ⚙️ Advanced Configuration

### 🎚️ Fine-Tuning Parameters

```csharp
var runner = OpenGARunner<T>
    .Init(population)
    .Epochs(1000)                    // Maximum generations
    .MaxDuration(TimeSpan.FromMinutes(5)) // Time-based termination
    .MaxPopulationSize(200)          // Population size control
    .MutationRate(0.1f)              // 10% mutation rate
    .CrossoverRate(0.8f)             // 80% crossover rate
    .ApplyTerminationStrategy(c => 
        c.ApplyTargetStandardDeviationTerminationStrategy(0.001)) // Convergence-based termination
```

### 🔧 Reproduction Selector Configuration

```csharp
// Apply a single reproduction selector
.ApplyReproductionSelector(c => c.ApplyTournamentReproductionSelector(tournamentSize: 3))
```

### 📈 Progress Monitoring

```csharp
var runner = OpenGARunner<int>.Init(population);

// Track progress during evolution
while (!runner.IsCompleted)
{
    var currentGeneration = runner.Step(); // Run one generation
    var bestFitness = currentGeneration.Max(c => c.Fitness);
    var avgFitness = currentGeneration.Average(c => c.Fitness);
    
    Console.WriteLine($"Generation {runner.CurrentEpoch}: Best={bestFitness:F4}, Avg={avgFitness:F4}");
}
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


## 🛠️ Extensibility

Create your own strategies by inheriting from base classes:

```csharp
// Custom crossover strategy
public class MyCustomCrossover<T> : BaseCrossoverStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> Crossover(
        Couple<T> couple, Random random)
    {
        // Your custom crossover logic
        yield return offspring;
    }
}

// Custom replacement strategy  
public class MyReplacementStrategy<T> : BaseReplacementStrategy<T>
{
    protected internal override IEnumerable<Chromosome<T>> SelectChromosomesForElimination(
        Chromosome<T>[] population, Chromosome<T>[] offspring, Random random)
    {
        // Your custom replacement logic
        return chromosomesToEliminate;
    }
}
```

---

## 🤝 Contributing

We welcome contributions! OpenGA.Net is built by the community, for the community.

### 🐛 **Found a Bug?**
[Open an issue](https://github.com/asarnaout/OpenGeneticAlgorithm.Net/issues) with:
- Clear reproduction steps
- Expected vs actual behavior
- Environment details

### 💡 **Have an Idea?**
[Start a discussion](https://github.com/asarnaout/OpenGeneticAlgorithm.Net/discussions) to:
- Propose new features
- Share use cases
- Get feedback from maintainers

### 🔧 **Want to Code?**
1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Make your changes with tests
4. Submit a pull request

See [CONTRIBUTING.md](CONTRIBUTING.md) for detailed guidelines.

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