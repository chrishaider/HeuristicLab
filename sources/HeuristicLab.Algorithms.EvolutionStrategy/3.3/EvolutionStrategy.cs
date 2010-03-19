#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2010 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
 *
 * This file is part of HeuristicLab.
 *
 * HeuristicLab is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * HeuristicLab is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with HeuristicLab. If not, see <http://www.gnu.org/licenses/>.
 */
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.RealVectorEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;

namespace HeuristicLab.Algorithms.EvolutionStrategy {
  /// <summary>
  /// A standard genetic algorithm.
  /// </summary>
  [Item("EvolutionStrategy", "An evolution strategy.")]
  [Creatable("Algorithms")]
  [StorableClass]
  public sealed class EvolutionStrategy : EngineAlgorithm {
    #region Problem Properties
    public override Type ProblemType {
      get { return typeof(ISingleObjectiveProblem); }
    }
    public new ISingleObjectiveProblem Problem {
      get { return (ISingleObjectiveProblem)base.Problem; }
      set { base.Problem = value; }
    }
    #endregion

    #region Parameter Properties
    private ValueParameter<IntValue> SeedParameter {
      get { return (ValueParameter<IntValue>)Parameters["Seed"]; }
    }
    private ValueParameter<BoolValue> SetSeedRandomlyParameter {
      get { return (ValueParameter<BoolValue>)Parameters["SetSeedRandomly"]; }
    }
    private ValueParameter<IntValue> PopulationSizeParameter {
      get { return (ValueParameter<IntValue>)Parameters["PopulationSize"]; }
    }
    private ValueParameter<IntValue> ParentsPerChildParameter {
      get { return (ValueParameter<IntValue>)Parameters["ParentsPerChild"]; }
    }
    private ValueParameter<IntValue> ChildrenParameter {
      get { return (ValueParameter<IntValue>)Parameters["Children"]; }
    }
    private ValueParameter<IntValue> MaximumGenerationsParameter {
      get { return (ValueParameter<IntValue>)Parameters["MaximumGenerations"]; }
    }
    private ValueParameter<DoubleMatrix> StrategyVectorBoundsParameter {
      get { return (ValueParameter<DoubleMatrix>)Parameters["StrategyVectorBounds"]; }
    }
    private ValueParameter<IntValue> ProblemDimensionParameter {
      get { return (ValueParameter<IntValue>)Parameters["ProblemDimension"]; }
    }
    private ValueParameter<DoubleValue> GeneralLearningRateParameter {
      get { return (ValueParameter<DoubleValue>)Parameters["GeneralLearningRate"]; }
    }
    private ValueParameter<DoubleValue> LearningRateParameter {
      get { return (ValueParameter<DoubleValue>)Parameters["LearningRate"]; }
    }
    private ValueParameter<BoolValue> PlusSelectionParameter {
      get { return (ValueParameter<BoolValue>)Parameters["PlusSelection"]; }
    }
    private OptionalConstrainedValueParameter<IManipulator> MutatorParameter {
      get { return (OptionalConstrainedValueParameter<IManipulator>)Parameters["Mutator"]; }
    }
    private ConstrainedValueParameter<ICrossover> RecombinatorParameter {
      get { return (ConstrainedValueParameter<ICrossover>)Parameters["Recombinator"]; }
    }
    #endregion

    #region Properties
    public IntValue Seed {
      get { return SeedParameter.Value; }
      set { SeedParameter.Value = value; }
    }
    public BoolValue SetSeedRandomly {
      get { return SetSeedRandomlyParameter.Value; }
      set { SetSeedRandomlyParameter.Value = value; }
    }
    public IntValue PopulationSize {
      get { return PopulationSizeParameter.Value; }
      set { PopulationSizeParameter.Value = value; }
    }
    public IntValue ParentsPerChild {
      get { return ParentsPerChildParameter.Value; }
      set { ParentsPerChildParameter.Value = value; }
    }
    public IntValue Children {
      get { return ChildrenParameter.Value; }
      set { ChildrenParameter.Value = value; }
    }
    public IntValue MaximumGenerations {
      get { return MaximumGenerationsParameter.Value; }
      set { MaximumGenerationsParameter.Value = value; }
    }
    private DoubleMatrix StrategyVectorBounds {
      get { return StrategyVectorBoundsParameter.Value; }
      set { StrategyVectorBoundsParameter.Value = value; }
    }
    private IntValue ProblemDimension {
      get { return ProblemDimensionParameter.Value; }
      set { ProblemDimensionParameter.Value = value; }
    }
    private DoubleValue GeneralLearningRate {
      get { return GeneralLearningRateParameter.Value; }
      set { GeneralLearningRateParameter.Value = value; }
    }
    private DoubleValue LearningRate {
      get { return LearningRateParameter.Value; }
      set { LearningRateParameter.Value = value; }
    }
    private BoolValue PlusSelection {
      get { return PlusSelectionParameter.Value; }
      set { PlusSelectionParameter.Value = value; }
    }
    public IManipulator Mutator {
      get { return MutatorParameter.Value; }
      set { MutatorParameter.Value = value; }
    }
    public ICrossover Recombinator {
      get { return RecombinatorParameter.Value; }
      set { RecombinatorParameter.Value = value; }
    }

    private RandomCreator RandomCreator {
      get { return (RandomCreator)OperatorGraph.InitialOperator; }
    }
    private SolutionsCreator SolutionsCreator {
      get { return (SolutionsCreator)RandomCreator.Successor; }
    }
    private EvolutionStrategyMainLoop MainLoop {
      get { return (EvolutionStrategyMainLoop)((UniformSequentialSubScopesProcessor)SolutionsCreator.Successor).Successor; }
    }
    #endregion

    public EvolutionStrategy()
      : base() {
      Parameters.Add(new ValueParameter<IntValue>("Seed", "The random seed used to initialize the new pseudo random number generator.", new IntValue(0)));
      Parameters.Add(new ValueParameter<BoolValue>("SetSeedRandomly", "True if the random seed should be set to a random value, otherwise false.", new BoolValue(true)));
      Parameters.Add(new ValueParameter<IntValue>("PopulationSize", "µ (mu) - the size of the population.", new IntValue(5)));
      Parameters.Add(new ValueParameter<IntValue>("ParentsPerChild", "ρ (rho) - how many parents should be recombined.", new IntValue(1)));
      Parameters.Add(new ValueParameter<IntValue>("Children", "λ (lambda) - the size of the offspring population.", new IntValue(10)));
      Parameters.Add(new ValueParameter<IntValue>("MaximumGenerations", "The maximum number of generations which should be processed.", new IntValue(1000)));
      Parameters.Add(new ValueParameter<DoubleMatrix>("StrategyVectorBounds", "2 column matrix with one row for each dimension specifying upper and lower bound for the strategy vector. If there are less rows than dimensions, the strategy vector will be read in a cycle.", new DoubleMatrix(new double[,] { {0.1, 5} })));
      Parameters.Add(new ValueParameter<IntValue>("ProblemDimension", "The problem dimension (length of the strategy vector.", new IntValue(1)));
      Parameters.Add(new ValueParameter<DoubleValue>("GeneralLearningRate", "τ0 (tau0) - the factor with which adjustments in the strategy vector is dampened over all dimensions. Recommendation is to use 1/Sqrt(2*ProblemDimension).", new DoubleValue(0.707106)));
      Parameters.Add(new ValueParameter<DoubleValue>("LearningRate", "τ (tau) - the factor with which adjustments in the strategy vector are dampened in a single dimension. Recommendation is to use 1/Sqrt(2*Sqrt(ProblemDimension)).", new DoubleValue(0.707106)));
      Parameters.Add(new ValueParameter<BoolValue>("PlusSelection", "True for plus selection (elitist population), false for comma selection (non-elitist population).", new BoolValue(true)));
      Parameters.Add(new ConstrainedValueParameter<ICrossover>("Recombinator", "The operator used to cross solutions."));
      Parameters.Add(new OptionalConstrainedValueParameter<IManipulator>("Mutator", "The operator used to mutate solutions."));

      RandomCreator randomCreator = new RandomCreator();
      SolutionsCreator solutionsCreator = new SolutionsCreator();
      UniformSequentialSubScopesProcessor strategyVectorProcessor = new UniformSequentialSubScopesProcessor();
      UniformRandomRealVectorCreator strategyVectorCreator = new UniformRandomRealVectorCreator();
      EvolutionStrategyMainLoop mainLoop = new EvolutionStrategyMainLoop();
      OperatorGraph.InitialOperator = randomCreator;

      randomCreator.RandomParameter.ActualName = "Random";
      randomCreator.SeedParameter.ActualName = SeedParameter.Name;
      randomCreator.SeedParameter.Value = null;
      randomCreator.SetSeedRandomlyParameter.ActualName = SetSeedRandomlyParameter.Name;
      randomCreator.SetSeedRandomlyParameter.Value = null;
      randomCreator.Successor = solutionsCreator;

      solutionsCreator.NumberOfSolutionsParameter.ActualName = PopulationSizeParameter.Name;
      solutionsCreator.Successor = strategyVectorProcessor;

      strategyVectorProcessor.Operator = strategyVectorCreator;
      strategyVectorProcessor.Successor = mainLoop;

      strategyVectorCreator.BoundsParameter.ActualName = StrategyVectorBoundsParameter.Name;
      strategyVectorCreator.LengthParameter.ActualName = ProblemDimensionParameter.Name;
      strategyVectorCreator.RandomParameter.ActualName = "Random";
      strategyVectorCreator.RealVectorParameter.ActualName = "StrategyVector";

      mainLoop.RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
      mainLoop.PopulationSizeParameter.ActualName = PopulationSizeParameter.Name;
      mainLoop.ParentsPerChildParameter.ActualName = ParentsPerChildParameter.Name;
      mainLoop.ChildrenParameter.ActualName = ChildrenParameter.Name;
      mainLoop.MaximumGenerationsParameter.ActualName = MaximumGenerationsParameter.Name;
      mainLoop.MutatorParameter.ActualName = MutatorParameter.Name;
      mainLoop.RecombinatorParameter.ActualName = RecombinatorParameter.Name;
      mainLoop.ResultsParameter.ActualName = "Results";

      Initialze();
    }
    [StorableConstructor]
    private EvolutionStrategy(bool deserializing) : base() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      EvolutionStrategy clone = (EvolutionStrategy)base.Clone(cloner);
      clone.Initialze();
      return clone;
    }

    #region Events
    protected override void OnProblemChanged() {
      ParameterizeStochasticOperator(Problem.SolutionCreator);
      ParameterizeStochasticOperator(Problem.Evaluator);
      ParameterizeStochasticOperator(Problem.Visualizer);
      foreach (IOperator op in Problem.Operators) ParameterizeStochasticOperator(op);
      ParameterizeSolutionsCreator();
      ParameterizeMainLoop();
      UpdateRecombinators();
      UpdateMutators();
      Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
      base.OnProblemChanged();
    }
    protected override void Problem_SolutionCreatorChanged(object sender, EventArgs e) {
      ParameterizeStochasticOperator(Problem.SolutionCreator);
      ParameterizeSolutionsCreator();
      base.Problem_SolutionCreatorChanged(sender, e);
    }
    protected override void Problem_EvaluatorChanged(object sender, EventArgs e) {
      ParameterizeStochasticOperator(Problem.Evaluator);
      ParameterizeSolutionsCreator();
      ParameterizeMainLoop();
      Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
      base.Problem_EvaluatorChanged(sender, e);
    }
    protected override void Problem_VisualizerChanged(object sender, EventArgs e) {
      ParameterizeStochasticOperator(Problem.Visualizer);
      ParameterizeMainLoop();
      base.Problem_VisualizerChanged(sender, e);
    }
    protected override void Problem_OperatorsChanged(object sender, EventArgs e) {
      foreach (IOperator op in Problem.Operators) ParameterizeStochasticOperator(op);
      UpdateRecombinators();
      UpdateMutators();
      base.Problem_OperatorsChanged(sender, e);
    }
    private void PopulationSizeParameter_ValueChanged(object sender, EventArgs e) {
      PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
    }
    private void PopulationSize_ValueChanged(object sender, EventArgs e) {
    }
    private void Evaluator_QualityParameter_ActualNameChanged(object sender, EventArgs e) {
      ParameterizeMainLoop();
    }
    #endregion

    #region Helpers
    [StorableHook(HookType.AfterDeserialization)]
    private void Initialze() {
      PopulationSizeParameter.ValueChanged += new EventHandler(PopulationSizeParameter_ValueChanged);
      PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
      if (Problem != null)
        Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
    }

    private void ParameterizeSolutionsCreator() {
      SolutionsCreator.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
      SolutionsCreator.SolutionCreatorParameter.ActualName = Problem.SolutionCreatorParameter.Name;
    }
    private void ParameterizeMainLoop() {
      MainLoop.BestKnownQualityParameter.ActualName = Problem.BestKnownQualityParameter.Name;
      MainLoop.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
      MainLoop.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
      MainLoop.QualityParameter.ActualName = Problem.Evaluator.QualityParameter.ActualName;
      MainLoop.VisualizerParameter.ActualName = Problem.VisualizerParameter.Name;
      MainLoop.VisualizationParameter.ActualName = Problem.Visualizer.VisualizationParameter.ActualName;
    }
    private void ParameterizeStochasticOperator(IOperator op) {
      if (op is IStochasticOperator)
        ((IStochasticOperator)op).RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
    }
    private void UpdateRecombinators() {
      ICrossover oldRecombinator = RecombinatorParameter.Value;
      RecombinatorParameter.ValidValues.Clear();
      foreach (ICrossover recombinator in Problem.Operators.OfType<ICrossover>().OrderBy(x => x.Name))
        RecombinatorParameter.ValidValues.Add(recombinator);
      if (oldRecombinator != null) {
        ICrossover recombinator = RecombinatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldRecombinator.GetType());
        if (recombinator != null) RecombinatorParameter.Value = recombinator;
      }
    }
    private void UpdateMutators() {
      IManipulator oldMutator = MutatorParameter.Value;
      MutatorParameter.ValidValues.Clear();
      foreach (IManipulator mutator in Problem.Operators.OfType<IManipulator>().OrderBy(x => x.Name))
        MutatorParameter.ValidValues.Add(mutator);
      if (oldMutator != null) {
        IManipulator mutator = MutatorParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldMutator.GetType());
        if (mutator != null) MutatorParameter.Value = mutator;
      }
    }
    #endregion
  }
}
