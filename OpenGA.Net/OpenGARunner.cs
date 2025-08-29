using OpenGA.Net.Exceptions;
using OpenGA.Net.ParentSelectors;
using OpenGA.Net.CrossoverStrategies;
using OpenGA.Net.SurvivorSelectionStrategies;
using OpenGA.Net.Termination;
using OpenGA.Net.OperatorSelectionPolicies;
using OpenGA.Net.Extensions;
using System.Diagnostics;

namespace OpenGA.Net;

public class OpenGARunner<T>
{
    internal int CurrentEpoch = 0;

    internal Stopwatch StopWatch = new();

    private int _maxNumberOfChromosomes;

    private int _minNumberOfChromosomes;

    private float _mutationRate = 0.2f;

    private readonly ParentSelectorRegistration<T> _parentSelectorRegistration = new();

    private readonly CrossoverStrategyRegistration<T> _crossoverStrategyRegistration = new();

    private readonly SurvivorSelectionStrategyRegistration<T> _survivorSelectionStrategyRegistration = new();

    private readonly TerminationStrategyConfiguration<T> _terminationStrategyConfig = new();

    private Chromosome<T>[] Population { get; set; } = [];

    internal double HighestFitness => Population.Max(c => c.Fitness);

    /// <summary>
    /// Gets the current state of the genetic algorithm including epoch, duration, and fitness metrics.
    /// </summary>
    internal GeneticAlgorithmState CurrentState => new(CurrentEpoch, StopWatch, HighestFitness);

    private readonly Random _random = new();

    private OpenGARunner() { }

    /// <summary>
    /// This method is used to initialize the GA Runner. The method expects a population, that is a collection
    /// of chromosomes where each chromosome represents a random solution to the problem at hand.
    /// </summary>
    /// <param name="initialPopulation">The initial population of chromosomes</param>
    /// <param name="minPopulationPercentage">Minimum population size as a percentage of initial population (0.0 to 1.0, default: 0.5 = 50%)</param>
    /// <param name="maxPopulationPercentage">Maximum population size as a percentage of initial population (1.0+, default: 2.0 = 200%)</param>
    /// <returns>A configured OpenGARunner instance</returns>
    /// <exception cref="MissingInitialPopulationException">Thrown when initial population is empty</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when percentage parameters are out of valid range</exception>
    public static OpenGARunner<T> Initialize(Chromosome<T>[] initialPopulation, float minPopulationPercentage = 0.5f, float maxPopulationPercentage = 2.0f)
    {
        if (initialPopulation is [])
        {
            throw new MissingInitialPopulationException("Initial population cannot be empty.");
        }

        if (minPopulationPercentage <= 0.0f || minPopulationPercentage > 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(minPopulationPercentage), "Minimum population percentage must be between 0.0 and 1.0.");
        }

        if (maxPopulationPercentage < 1.0f)
        {
            throw new ArgumentOutOfRangeException(nameof(maxPopulationPercentage), "Maximum population percentage must be 1.0 or greater.");
        }

        if (minPopulationPercentage >= maxPopulationPercentage)
        {
            throw new ArgumentOutOfRangeException(nameof(minPopulationPercentage), "Minimum population percentage must be less than maximum population percentage.");
        }

        var initialPopulationSize = initialPopulation.Length;
        var minPopulationSize = Math.Max(1, (int)(initialPopulationSize * minPopulationPercentage));
        var maxPopulationSize = (int)(initialPopulationSize * maxPopulationPercentage);

        return new OpenGARunner<T>
        {
            Population = initialPopulation,
            _minNumberOfChromosomes = minPopulationSize,
            _maxNumberOfChromosomes = maxPopulationSize
        };
    }

    /// <summary>
    /// The likelihood that a chromosome will mutate within an epoch/generation. Defaults to 0.2 (20%).
    /// </summary>
    /// <param name="mutationRate">Value should be between 0 and 1, where 0 indicates that no mutation will occur while 1 indicates that mutation will happen 100% of the time.</param>
    public OpenGARunner<T> MutationRate(float mutationRate)
    {
        if (mutationRate < 0 || mutationRate > 1)
        {
            throw new ArgumentOutOfRangeException(nameof(mutationRate), "Value must be between 0 and 1.");
        }

        _mutationRate = mutationRate;
        return this;
    }

    public OpenGARunner<T> ParentSelection(Action<ParentSelectorRegistration<T>> selectorConfigurator)
    {
        ArgumentNullException.ThrowIfNull(selectorConfigurator, nameof(selectorConfigurator));

        selectorConfigurator(_parentSelectorRegistration);
        return this;
    }

    public OpenGARunner<T> Crossover(Action<CrossoverStrategyRegistration<T>> crossoverStrategyRegistration)
    {
        crossoverStrategyRegistration(_crossoverStrategyRegistration);

        return this;
    }

    public OpenGARunner<T> SurvivorSelection(Action<SurvivorSelectionStrategyRegistration<T>> survivorSelectionStrategyRegistration)
    {
        ArgumentNullException.ThrowIfNull(survivorSelectionStrategyRegistration, nameof(survivorSelectionStrategyRegistration));

        survivorSelectionStrategyRegistration(_survivorSelectionStrategyRegistration);

        return this;
    }

    /// <summary>
    /// Configures termination strategies that determine when the genetic algorithm should stop.
    /// </summary>
    /// <param name="terminationStrategyConfigurators">One or more configuration actions for termination strategies.</param>
    /// <returns>The OpenGARunner instance for method chaining.</returns>
    /// <remarks>
    /// Termination strategies define the conditions under which the genetic algorithm should halt execution.
    /// Multiple strategies can be configured, and the algorithm will stop when any of them are met.
    /// 
    /// Available termination strategies include:
    /// - Maximum epochs: Stop after a specified number of generations
    /// - Maximum duration: Stop after a specified time period
    /// - Target standard deviation: Stop when population diversity drops below a threshold
    /// - Target fitness: Stop when a chromosome achieves the desired fitness value
    /// 
    /// Example usage:
    /// <code>
    /// .Termination(config => config.MaximumEpochs(100).MaximumDuration(TimeSpan.FromMinutes(5)))
    /// </code>
    /// </remarks>
    public OpenGARunner<T> Termination(Action<TerminationStrategyConfiguration<T>> terminationStrategyConfigurator)
    {
        ArgumentNullException.ThrowIfNull(terminationStrategyConfigurator, nameof(terminationStrategyConfigurator));

        terminationStrategyConfigurator(_terminationStrategyConfig);

        return this;
    }

    /// <summary>
    /// Applies default strategies for any components that haven't been explicitly configured.
    /// 
    /// This method ensures the genetic algorithm has all required components by applying
    /// sensible defaults when the user hasn't specified certain strategies. It also
    /// intelligently configures operator selection policies based on the number of
    /// crossover strategies available and whether custom weights are configured.
    /// 
    /// Operator selection policy precedence:
    /// 1. If explicitly configured policy exists → use it (validates compatibility with custom weights)
    /// 2. If any operator has custom weight > 0 → use CustomWeightPolicy
    /// 3. If single strategy → use FirstChoicePolicy
    /// 4. If multiple strategies → use AdaptivePursuitPolicy
    /// </summary>
    /// <exception cref="OperatorSelectionPolicyConflictException">
    /// Thrown when custom weights are configured but a non-CustomWeight operator selection policy is applied.
    /// </exception>
    private void DefaultMissingStrategies()
    {
        _parentSelectorRegistration.ValidateAndDefault();

        if (_terminationStrategyConfig.TerminationStrategies is [])
        {
            _terminationStrategyConfig.MaximumEpochs(100);
        }

        _crossoverStrategyRegistration.ValidateAndDefault();
        
        _survivorSelectionStrategyRegistration.ValidateAndDefault();
    }

    /// <summary>
    /// Updates the Adaptive Pursuit Policy algorithm with performance feedback from a crossover operation.
    /// </summary>
    private void UpdateAdaptivePursuitReward(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseCrossoverStrategy<T> crossoverStrategy,
        Couple<T> couple,
        IEnumerable<Chromosome<T>> offspring)
    {
        var offspringList = offspring.ToList();
        if (offspringList.Count == 0)
        {
            return; // No offspring to evaluate
        }

        // Calculate best fitness among parents and offspring
        var bestParentFitness = Math.Max(couple.IndividualA.Fitness, couple.IndividualB.Fitness);
        var bestOffspringFitness = offspringList.Max(o => o.Fitness);

        // Calculate population fitness range for normalization
        var populationFitnesses = Population.Select(c => c.Fitness).ToArray();
        var populationFitnessRange = populationFitnesses.Max() - populationFitnesses.Min();

        // Calculate diversity among offspring (standard deviation of fitness)
        var offspringFitnesses = offspringList.Select(o => o.Fitness);
        var offspringDiversity = offspringFitnesses.StandardDeviation();

        // Update the reward for this crossover strategy
        adaptivePursuit.UpdateReward(
            crossoverStrategy,
            bestParentFitness,
            bestOffspringFitness,
            populationFitnessRange,
            offspringDiversity);
    }

    /// <summary>
    /// Updates the Adaptive Pursuit Policy with performance feedback from a survivor selection operation.
    /// 
    /// Rationale:
    /// - Survivor selection quality is better captured by changes in the population as a whole, not only the single best individual.
    /// - We therefore use mean fitness improvement from pre- to post-survivor selection as the primary reward signal.
    /// - To encourage maintaining healthy exploration, we add a diversity component based on the change in fitness standard deviation.
    /// - Metrics are computed immediately after survivor selection and before mutation/repair to isolate the survivor selection effect.
    /// </summary>
    private static void UpdateAdaptivePursuitRewardForSurvivorSelection(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseSurvivorSelectionStrategy<T> survivorSelectionStrategy,
        Chromosome<T>[] preSurvivorSelectionPopulation,
        Chromosome<T>[] postSurvivorSelectionPopulation)
    {
        // Fitness arrays
        var preFitnesses = preSurvivorSelectionPopulation.Select(c => c.Fitness).ToArray();
        var postFitnesses = postSurvivorSelectionPopulation.Select(c => c.Fitness).ToArray();

        // Primary signal: mean fitness improvement across the whole population
        var preMean = preFitnesses.Average();
        var postMean = postFitnesses.Average();

        // Normalization scale: use the fitness range of the pre-survivor selection population to keep scale consistent
        var preRange = preFitnesses.Max() - preFitnesses.Min();

        // Diversity component: change in standard deviation (positive favors exploration)
        var preStd = preFitnesses.StandardDeviation();
        var postStd = postFitnesses.StandardDeviation();
        var diversityDelta = postStd - preStd;

        // Feed the signals to Adaptive Pursuit. We map mean improvement to the primary reward path,
        // and use diversity change as the auxiliary term (internally weighted by diversityWeight).
        adaptivePursuit.UpdateReward(
            survivorSelectionStrategy,
            preMean,
            postMean,
            preRange,
            diversityDelta);
    }

    /// <summary>
    /// Updates the Adaptive Pursuit Policy with performance feedback from a parent selection operation.
    /// 
    /// Rationale:
    /// - Parent selection quality is measured by evaluating the fitness characteristics of the selected couples
    /// - We reward parent selectors that choose high-fitness parents, which should lead to better offspring
    /// - We also consider diversity in the selected couples to encourage exploration
    /// - The fitness range and diversity of selected couples are compared against the overall population
    /// </summary>
    private static void UpdateAdaptivePursuitRewardForParentSelection(
        AdaptivePursuitPolicy adaptivePursuit,
        BaseParentSelector<T> parentSelector,
        Chromosome<T>[] population,
        IEnumerable<Couple<T>> selectedCouples)
    {
        var couplesList = selectedCouples.ToList();
        if (couplesList.Count == 0)
        {
            return; // No couples to evaluate
        }

        // Calculate population fitness metrics for normalization
        var populationFitnesses = population.Select(c => c.Fitness).ToArray();
        var populationMean = populationFitnesses.Average();
        var populationRange = populationFitnesses.Max() - populationFitnesses.Min();

        // Calculate metrics for selected couples
        var selectedParentFitnesses = couplesList
            .SelectMany(couple => new[] { couple.IndividualA.Fitness, couple.IndividualB.Fitness })
            .ToArray();

        var selectedMean = selectedParentFitnesses.Average();
        var selectedDiversity = selectedParentFitnesses.StandardDeviation();

        // Reward selection of high-fitness parents (primary signal: how much above population mean)
        var fitnessAdvantage = selectedMean - populationMean;

        // Diversity bonus: reward maintaining some diversity in selection
        var diversityBonus = selectedDiversity;

        // Update the reward for this parent selector strategy
        adaptivePursuit.UpdateReward(
            parentSelector,
            populationMean,
            selectedMean,
            populationRange,
            diversityBonus);
    }

    /// <summary>
    /// Executes the genetic algorithm until one of the configured termination conditions is met.
    /// 
    /// This method runs the complete genetic algorithm process, including:
    /// - Validating that all required strategies (parent selection, crossover, survivor selection) are configured
    /// - Applying a default termination strategy (100 epochs) if none is specified
    /// - Iteratively evolving the population through selection, crossover, mutation, and survivor selection
    /// - Tracking algorithm state including current epoch, duration, and fitness metrics
    /// - Terminating when any configured termination condition is satisfied
    /// 
    /// The algorithm follows these steps each generation:
    /// 1. Check termination conditions
    /// 2. Generate offspring through reproduction selection and crossover
    /// 3. Apply survivor selection strategy to create new population
    /// 4. Apply mutation and genetic repair to all chromosomes
    /// 5. Update chromosome ages and reset offspring ages
    /// </summary>
    /// <returns>
    /// The chromosome with the highest fitness value from the final population.
    /// This represents the best solution found by the genetic algorithm.
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// Thrown when the calculated number of offspring is invalid (≤ 0 or > 2× population size).
    /// </exception>
    public Chromosome<T> RunToCompletion()
    {
        StopWatch.Start();
        DefaultMissingStrategies();

        for (; ; CurrentEpoch++)
        {
            if (_terminationStrategyConfig.ShouldTerminate(CurrentState))
            {
                break;
            }

            List<Chromosome<T>> offspring = [];

            var survivorSelectionSelectionPolicy = _survivorSelectionStrategyRegistration.GetSurvivorSelectionSelectionPolicy();
            var survivorSelectionStrategy = (BaseSurvivorSelectionStrategy<T>)survivorSelectionSelectionPolicy.SelectOperator(_random, CurrentEpoch);

            var requiredNumberOfOffspring = CalculateOptimalOffspringCount(survivorSelectionStrategy);

            var maxCouplesPerBatch = Math.Max(requiredNumberOfOffspring, _maxNumberOfChromosomes);

            while (offspring.Count < requiredNumberOfOffspring)
            {
                var remainingOffspringNeeded = requiredNumberOfOffspring - offspring.Count;
                var couplesForThisBatch = Math.Min(maxCouplesPerBatch, remainingOffspringNeeded * 2); // Generate extra to account for failed crossovers
                
                var parentSelectorPolicy = _parentSelectorRegistration.GetParentSelectorSelectionPolicy();
                var parentSelector = (BaseParentSelector<T>)parentSelectorPolicy.SelectOperator(_random, CurrentEpoch);
                var couples = parentSelector.SelectMatingPairs(Population, _random, couplesForThisBatch, CurrentEpoch);

                if (parentSelectorPolicy is AdaptivePursuitPolicy parentAdaptivePursuit)
                {
                    UpdateAdaptivePursuitRewardForParentSelection(parentAdaptivePursuit, parentSelector, Population, couples);
                }

                var offspringGeneratedInBatch = 0;

                foreach (var couple in couples)
                {
                    if (offspring.Count >= requiredNumberOfOffspring)
                    {
                        break;
                    }

                    var crossoverPolicy = _crossoverStrategyRegistration.GetCrossoverSelectionPolicy();

                    var crossoverStrategy = (BaseCrossoverStrategy<T>)crossoverPolicy.SelectOperator(_random, CurrentEpoch);

                    var crossoverRate = crossoverStrategy.CrossoverRateOverride ?? _crossoverStrategyRegistration.GetCrossoverRate();

                    if (_random.NextDouble() <= crossoverRate)
                    {
                        var newOffspring = crossoverStrategy.Crossover(couple, _random);

                        foreach (var child in newOffspring)
                        {
                            child.InvalidateFitness();
                        }

                        if (crossoverPolicy is AdaptivePursuitPolicy adaptivePursuit)
                        {
                            UpdateAdaptivePursuitReward(adaptivePursuit, crossoverStrategy, couple, newOffspring);
                        }

                        offspring.AddRange(newOffspring);
                        offspringGeneratedInBatch += newOffspring.Count();
                    }
                }

                if (offspringGeneratedInBatch == 0)
                {
                    break;
                }
            }

            // Keep a snapshot to isolate survivor selection impact (copy array to avoid in-place modifications)
            var preSurvivorSelectionPopulation = Population.ToArray();

            Population = survivorSelectionStrategy.ApplySurvivorSelection(Population, [.. offspring], _random, CurrentEpoch);

            // Update Adaptive Pursuit for survivor selection based on immediate post-survivor selection population (before mutation)
            if (survivorSelectionSelectionPolicy is AdaptivePursuitPolicy adaptiveSurvivorSelection)
            {
                UpdateAdaptivePursuitRewardForSurvivorSelection(adaptiveSurvivorSelection, survivorSelectionStrategy, preSurvivorSelectionPopulation, Population);
            }

            foreach (var chromosome in Population)
            {
                if (_random.NextDouble() <= _mutationRate)
                {
                    chromosome.Mutate();
                    chromosome.InvalidateFitness();
                }

                chromosome.GeneticRepair();
                chromosome.InvalidateFitness();
                chromosome.IncrementAge();
            }

            foreach (var child in offspring)
            {
                child.ResetAge();
            }
        }

        StopWatch.Stop();

        return Population.OrderByDescending(c => c.Fitness).First();
    }

    private int CalculateOptimalOffspringCount(BaseSurvivorSelectionStrategy<T> survivorSelectionStrategy)
    {
        int result;
        var currentPopulationSize = Population.Length;

        var customOverride = _survivorSelectionStrategyRegistration.GetOffspringGenerationRateOverride();
        if (customOverride.HasValue)
        {
            result = Math.Max(1, (int)(currentPopulationSize * customOverride.Value));
        }
        else
        {
            result = Math.Max(1, (int)(currentPopulationSize * survivorSelectionStrategy.RecommendedOffspringGenerationRate));
        }

        // Ensure the result respects population bounds
        // The offspring count should be sufficient to potentially reach max population
        var maxAllowableOffspring = _maxNumberOfChromosomes - _minNumberOfChromosomes;
        var minRequiredOffspring = Math.Max(1, _minNumberOfChromosomes - currentPopulationSize);

        // Clamp the result within reasonable bounds
        result = Math.Max(minRequiredOffspring, Math.Min(result, maxAllowableOffspring));

        if (result <= 0)
        {
            throw new InvalidOperationException("Required number of offspring must be greater than zero. Check survivor selection strategy configuration.");
        }

        if (result > _maxNumberOfChromosomes * 2)
        {
            throw new InvalidOperationException($"Required number of offspring ({result}) exceeds reasonable bounds (maximum: {_maxNumberOfChromosomes * 2}). This may indicate an issue with the survivor selection strategy configuration.");
        }

        return result;
    }
}