# 🧬 OpenGeneticAlgorithm.NET

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

A **Chromosome** in OpenGA.Net represents a potential solution to your optimization problem. It contains **genes** (the solution components) and defines how to evaluate, modify, and repair solutions.

**Step 1:** Create your chromosome by inheriting from the `Chromosome<T>` abstract class, the example below demonstrates how a Chromosome representing a potential solution to the [traveling salesman problem](https://en.wikipedia.org/wiki/Travelling_salesman_problem) could look like:

```csharp
using OpenGA.Net;

// A chromosome for the Traveling Salesman Problem. Each chromosome represents a route through cities (genes = city sequence)
public class TspChromosome : Chromosome<int>
{
    public TspChromosome(IList<int> cities, double[,] distanceMatrix) : base(cities)
    {
    }
    
    // Calculate how "good" this route is (shorter distance = higher fitness)
    public override double CalculateFitness()
    {
        double totalDistance = CalculateTotalDistance();
        
        return 1.0 / (1.0 + totalDistance);
    }
    
    // Randomly swap two cities in the route
    public override void Mutate()
    {
        int index1 = _random.Next(Genes.Count);
        int index2 = _random.Next(Genes.Count);
        (Genes[index1], Genes[index2]) = (Genes[index2], Genes[index1]);
    }
    
    // Create an identical copy of this chromosome
    public override Chromosome<int> DeepCopy()
    {
        return new TspChromosome(new List<int>(Genes), _distanceMatrix);
    }
    
    // Ensure each city appears exactly once (fix any duplicates)
    public override void GeneticRepair()
    {
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
var bestSolution = OpenGARunner<int>
    .Initialize(initialPopulation)
    .RunToCompletion();

// Get your optimized result
Console.WriteLine($"Best route: {string.Join(" → ", bestSolution.Genes)}");
```

That's it! 🎉 You just solved an optimization problem with a few lines of code.

---

## 🏗️ Architecture Overview

OpenGA.Net is built around four core concepts that work together seamlessly:

### 🧬 **Chromosomes**
Your solution candidates. Strongly typed and extensible. Each `Chromosome<T>` subclass defines how a solution is represented and evaluated. Implement:

- CalculateFitness() — return a higher-is-better score for the current Genes
- Mutate() — randomly perturb Genes to explore the search space
- DeepCopy() — produce a full copy used during crossover
- GeneticRepair() (optional) — fix invalid states after crossover/mutation

Tip: See the TSP chromosome in the Quick Start above for a concrete example. The pattern is the same for any problem: choose a gene representation, define a fitness function, add a simple mutation, and optionally repair to enforce constraints.

### 🎯 **Reproduction Selectors**
Choose the best parents for the next generation:

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

**Selection Criteria:**
- **Gene Independence**: Use Uniform for independent genes, One-Point for clustered genes
- **Solution Representation**: Use K-Point for structured data, Custom for complex constraints
- **Performance**: One-Point is fastest, Uniform provides most diversity
- **Problem Complexity**: Simple problems → One-Point, Complex → Custom or Uniform

### 🔄 **Replacement Strategies**
Manage population evolution over generations:

| Strategy | When to Use | Convergence Speed | Population Diversity | Resource Constraints |
|----------|-------------|-------------------|---------------------|---------------------|
| **Elitist** *(Default)* | Most optimization problems | Fast convergence | Moderate diversity loss | Low computational overhead |
| **Generational** | Exploration-heavy search | Slower, thorough exploration | High diversity maintained | Higher memory usage |
| **Tournament** | Balanced performance | Moderate convergence | Good diversity balance | Moderate computational cost |
| **Age-based** | Long-running evolutionary systems | Very slow, stable | Excellent long-term diversity | Requires age tracking |
| **Boltzmann** | Temperature-controlled evolution | Adaptive convergence | Dynamic diversity control | Higher computational complexity |
| **Random Elimination** | Maintain population diversity | Slowest convergence | Maximum diversity | Minimal computational overhead |

**Replacement Strategy Quick Guide:**
- **Need fast convergence?** → Elitist
- **Avoiding local optima?** → Generational or Tournament  
- **Long-term evolution runs?** → Age-based
- **Limited computational resources?** → Random Elimination
- **Need adaptive control?** → Boltzmann

### 🏁 **Termination Strategies**
Control when the genetic algorithm stops evolving:

| Strategy | When to Use | Stopping Condition | Best For | Predictability |
|----------|-------------|-------------------|----------|----------------|
| **Maximum Epochs** *(Default)* | Known iteration limits | Fixed number of generations | Time-constrained scenarios | Highly predictable runtime |
| **Maximum Duration** | Real-time applications | Maximum execution duration | Production systems | Predictable time bounds |
| **Target Standard Deviation** | Diversity monitoring | Low population diversity | Avoiding premature convergence | Adaptive stopping |

**Termination Strategy Quick Guide:**
- **Default/Most common?** → Maximum Epochs *(automatically applied)*
- **Need predictable runtime?** → Maximum Epochs or Maximum Duration
- **Avoiding premature convergence?** → Target Standard Deviation
- **Production systems?** → Maximum Duration with fallback strategies

### 💡 **Strategy Selection Examples**

```csharp
// High-performance optimization (fast convergence needed)
.ApplyReproductionSelector(c => c.ApplyElitistReproductionSelector())
.ApplyCrossoverStrategies(c => c.ApplyOnePointCrossoverStrategy())
.ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy())
.Termination(c => c.MaximumEpochs(maxEpochs: 100))

// Exploratory search (avoiding local optima)
.ApplyReproductionSelector(c => c.ApplyTournamentReproductionSelector(tournamentSize: 5))
.ApplyCrossoverStrategies(c => c.ApplyUniformCrossoverStrategy())
.ApplyReplacementStrategy(c => c.ApplyGenerationalReplacementStrategy())
.Termination(c => c.TargetStandardDeviation(targetStandardDeviation: 0.001))

// Production system (time-constrained)
.ApplyReproductionSelector(c => c.ApplyTournamentReproductionSelector())
.ApplyCrossoverStrategies(c => c.ApplyOnePointCrossoverStrategy())
.ApplyReplacementStrategy(c => c.ApplyElitistReplacementStrategy())
.Termination(c => c.MaximumDuration(TimeSpan.FromMinutes(5)))

// Quality-focused research (target standard deviation termination)
.ApplyReproductionSelector(c => c.ApplyBoltzmannReproductionSelector(temperature: 100))
.ApplyCrossoverStrategies(c => c.ApplyKPointCrossoverStrategy(k: 3))
.ApplyReplacementStrategy(c => c.ApplyAgeBasedReplacementStrategy())
.Termination(c => c.TargetStandardDeviation(targetStandardDeviation: 0.001, window: 10))
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
        var parent1 = couple.Parent1;
        var parent2 = couple.Parent2;
        
        // Example: Custom blend crossover
        var offspring = new MyChromosome<T>();
        // Implement your crossover algorithm here
        
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
        // Example: Replace chromosomes based on custom criteria
        var toEliminate = population
            .OrderBy(c => CustomFitnessMetric(c))
            .Take(offspring.Length);
            
        return toEliminate;
    }
    
    private double CustomFitnessMetric<T>(Chromosome<T> chromosome)
    {
        // Your custom elimination criteria
        return chromosome.Fitness;
    }
}

// Custom reproduction selector
public class MyReproductionSelector<T> : BaseReproductionSelector<T>
{
    protected internal override IEnumerable<Chromosome<T>> SelectParents(
        Chromosome<T>[] population, int numberOfParents, Random random)
    {
        // Your custom parent selection logic
        // Example: Custom weighted selection
        for (int i = 0; i < numberOfParents; i++)
        {
            var selectedParent = CustomSelectionAlgorithm(population, random);
            yield return selectedParent;
        }
    }
    
    private Chromosome<T> CustomSelectionAlgorithm<T>(
        Chromosome<T>[] population, Random random)
    {
        // Implement your selection algorithm
        return population[random.Next(population.Length)];
    }
}

// Custom termination strategy
public class MyTerminationStrategy<T> : BaseTerminationStrategy<T>
{
    private readonly int _maxStagnantGenerations;
    private int _stagnantCount = 0;
    private double _lastBestFitness = double.MinValue;
    
    public MyTerminationStrategy(int maxStagnantGenerations)
    {
        _maxStagnantGenerations = maxStagnantGenerations;
    }
    
    public override bool Terminate(GeneticAlgorithmState state)
    {
        var currentBestFitness = state.HighestFitness;
        
        // Terminate if fitness hasn't improved for specified generations
        if (Math.Abs(currentBestFitness - _lastBestFitness) < 0.0001)
        {
            _stagnantCount++;
        }
        else
        {
            _stagnantCount = 0;
            _lastBestFitness = currentBestFitness;
        }
        
        return _stagnantCount >= _maxStagnantGenerations;
    }
}

// Using your custom strategies
var result = OpenGARunner<MyGeneType>
    .Initialize(initialPopulation)
    .ApplyReproductionSelector(c => c.ApplyCustomReproductionSelector(new MyReproductionSelector<MyGeneType>()))
    .ApplyCrossoverStrategies(c => c.ApplyCustomCrossoverStrategy(new MyCustomCrossover<MyGeneType>()))
    .ApplyReplacementStrategy(c => c.ApplyCustomReplacementStrategy(new MyReplacementStrategy<MyGeneType>()))
    .Termination(c => c.Custom(new MyTerminationStrategy<MyGeneType>(50)))
    .RunToCompletion();
```

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