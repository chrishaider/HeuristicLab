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
using HeuristicLab.Analysis;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Optimization.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Random;
using HeuristicLab.Common;

namespace HeuristicLab.Algorithms.OffspringSelectionGeneticAlgorithm {
  /// <summary>
  /// An offspring selection genetic algorithm.
  /// </summary>
  [Item("Offspring Selection Genetic Algorithm", "An offspring selection genetic algorithm (Affenzeller, M. et al. 2009. Genetic Algorithms and Genetic Programming - Modern Concepts and Practical Applications. CRC Press).")]
  [Creatable("Algorithms")]
  [StorableClass]
  public sealed class OffspringSelectionGeneticAlgorithm : EngineAlgorithm {
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
    private ConstrainedValueParameter<ISelector> SelectorParameter {
      get { return (ConstrainedValueParameter<ISelector>)Parameters["Selector"]; }
    }
    private ConstrainedValueParameter<ICrossover> CrossoverParameter {
      get { return (ConstrainedValueParameter<ICrossover>)Parameters["Crossover"]; }
    }
    private ValueParameter<PercentValue> MutationProbabilityParameter {
      get { return (ValueParameter<PercentValue>)Parameters["MutationProbability"]; }
    }
    private OptionalConstrainedValueParameter<IManipulator> MutatorParameter {
      get { return (OptionalConstrainedValueParameter<IManipulator>)Parameters["Mutator"]; }
    }
    private ValueParameter<IntValue> ElitesParameter {
      get { return (ValueParameter<IntValue>)Parameters["Elites"]; }
    }
    private ValueParameter<IntValue> MaximumGenerationsParameter {
      get { return (ValueParameter<IntValue>)Parameters["MaximumGenerations"]; }
    }
    private ValueLookupParameter<DoubleValue> SuccessRatioParameter {
      get { return (ValueLookupParameter<DoubleValue>)Parameters["SuccessRatio"]; }
    }
    private ValueLookupParameter<DoubleValue> ComparisonFactorLowerBoundParameter {
      get { return (ValueLookupParameter<DoubleValue>)Parameters["ComparisonFactorLowerBound"]; }
    }
    private ValueLookupParameter<DoubleValue> ComparisonFactorUpperBoundParameter {
      get { return (ValueLookupParameter<DoubleValue>)Parameters["ComparisonFactorUpperBound"]; }
    }
    private OptionalConstrainedValueParameter<IDiscreteDoubleValueModifier> ComparisonFactorModifierParameter {
      get { return (OptionalConstrainedValueParameter<IDiscreteDoubleValueModifier>)Parameters["ComparisonFactorModifier"]; }
    }
    private ValueLookupParameter<DoubleValue> MaximumSelectionPressureParameter {
      get { return (ValueLookupParameter<DoubleValue>)Parameters["MaximumSelectionPressure"]; }
    }
    private ValueLookupParameter<BoolValue> OffspringSelectionBeforeMutationParameter {
      get { return (ValueLookupParameter<BoolValue>)Parameters["OffspringSelectionBeforeMutation"]; }
    }
    private ValueLookupParameter<IntValue> SelectedParentsParameter {
      get { return (ValueLookupParameter<IntValue>)Parameters["SelectedParents"]; }
    }
    private ValueParameter<MultiAnalyzer> AnalyzerParameter {
      get { return (ValueParameter<MultiAnalyzer>)Parameters["Analyzer"]; }
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
    public ISelector Selector {
      get { return SelectorParameter.Value; }
      set { SelectorParameter.Value = value; }
    }
    public ICrossover Crossover {
      get { return CrossoverParameter.Value; }
      set { CrossoverParameter.Value = value; }
    }
    public PercentValue MutationProbability {
      get { return MutationProbabilityParameter.Value; }
      set { MutationProbabilityParameter.Value = value; }
    }
    public IManipulator Mutator {
      get { return MutatorParameter.Value; }
      set { MutatorParameter.Value = value; }
    }
    public IntValue Elites {
      get { return ElitesParameter.Value; }
      set { ElitesParameter.Value = value; }
    }
    public IntValue MaximumGenerations {
      get { return MaximumGenerationsParameter.Value; }
      set { MaximumGenerationsParameter.Value = value; }
    }
    public DoubleValue SuccessRatio {
      get { return SuccessRatioParameter.Value; }
      set { SuccessRatioParameter.Value = value; }
    }
    public DoubleValue ComparisonFactorLowerBound {
      get { return ComparisonFactorLowerBoundParameter.Value; }
      set { ComparisonFactorLowerBoundParameter.Value = value; }
    }
    public DoubleValue ComparisonFactorUpperBound {
      get { return ComparisonFactorUpperBoundParameter.Value; }
      set { ComparisonFactorUpperBoundParameter.Value = value; }
    }
    public IDiscreteDoubleValueModifier ComparisonFactorModifier {
      get { return ComparisonFactorModifierParameter.Value; }
      set { ComparisonFactorModifierParameter.Value = value; }
    }
    public DoubleValue MaximumSelectionPressure {
      get { return MaximumSelectionPressureParameter.Value; }
      set { MaximumSelectionPressureParameter.Value = value; }
    }
    public BoolValue OffspringSelectionBeforeMutation {
      get { return OffspringSelectionBeforeMutationParameter.Value; }
      set { OffspringSelectionBeforeMutationParameter.Value = value; }
    }
    public IntValue SelectedParents {
      get { return SelectedParentsParameter.Value; }
      set { SelectedParentsParameter.Value = value; }
    }
    public MultiAnalyzer Analyzer {
      get { return AnalyzerParameter.Value; }
      set { AnalyzerParameter.Value = value; }
    }
    private RandomCreator RandomCreator {
      get { return (RandomCreator)OperatorGraph.InitialOperator; }
    }
    private SolutionsCreator SolutionsCreator {
      get { return (SolutionsCreator)RandomCreator.Successor; }
    }
    private OffspringSelectionGeneticAlgorithmMainLoop MainLoop {
      get { return (OffspringSelectionGeneticAlgorithmMainLoop)SolutionsCreator.Successor; }
    }
    [Storable]
    private BestAverageWorstQualityAnalyzer qualityAnalyzer;
    [Storable]
    private ValueAnalyzer selectionPressureAnalyzer;
    #endregion

    [StorableConstructor]
    private OffspringSelectionGeneticAlgorithm(bool deserializing) : base(deserializing) { }
    public OffspringSelectionGeneticAlgorithm()
      : base() {
      Parameters.Add(new ValueParameter<IntValue>("Seed", "The random seed used to initialize the new pseudo random number generator.", new IntValue(0)));
      Parameters.Add(new ValueParameter<BoolValue>("SetSeedRandomly", "True if the random seed should be set to a random value, otherwise false.", new BoolValue(true)));
      Parameters.Add(new ValueParameter<IntValue>("PopulationSize", "The size of the population of solutions.", new IntValue(100)));
      Parameters.Add(new ConstrainedValueParameter<ISelector>("Selector", "The operator used to select solutions for reproduction."));
      Parameters.Add(new ConstrainedValueParameter<ICrossover>("Crossover", "The operator used to cross solutions."));
      Parameters.Add(new ValueParameter<PercentValue>("MutationProbability", "The probability that the mutation operator is applied on a solution.", new PercentValue(0.05)));
      Parameters.Add(new OptionalConstrainedValueParameter<IManipulator>("Mutator", "The operator used to mutate solutions."));
      Parameters.Add(new ValueParameter<IntValue>("Elites", "The numer of elite solutions which are kept in each generation.", new IntValue(1)));
      Parameters.Add(new ValueParameter<IntValue>("MaximumGenerations", "The maximum number of generations which should be processed.", new IntValue(1000)));
      Parameters.Add(new ValueLookupParameter<DoubleValue>("SuccessRatio", "The ratio of successful to total children that should be achieved.", new DoubleValue(1)));
      Parameters.Add(new ValueLookupParameter<DoubleValue>("ComparisonFactorLowerBound", "The lower bound of the comparison factor (start).", new DoubleValue(0)));
      Parameters.Add(new ValueLookupParameter<DoubleValue>("ComparisonFactorUpperBound", "The upper bound of the comparison factor (end).", new DoubleValue(1)));
      Parameters.Add(new OptionalConstrainedValueParameter<IDiscreteDoubleValueModifier>("ComparisonFactorModifier", "The operator used to modify the comparison factor.", new ItemSet<IDiscreteDoubleValueModifier>(new IDiscreteDoubleValueModifier[] { new LinearDiscreteDoubleValueModifier() }), new LinearDiscreteDoubleValueModifier()));
      Parameters.Add(new ValueLookupParameter<DoubleValue>("MaximumSelectionPressure", "The maximum selection pressure that terminates the algorithm.", new DoubleValue(100)));
      Parameters.Add(new ValueLookupParameter<BoolValue>("OffspringSelectionBeforeMutation", "True if the offspring selection step should be applied before mutation, false if it should be applied after mutation.", new BoolValue(false)));
      Parameters.Add(new ValueLookupParameter<IntValue>("SelectedParents", "How much parents should be selected each time the offspring selection step is performed until the population is filled. This parameter should be about the same or twice the size of PopulationSize for smaller problems, and less for large problems.", new IntValue(200)));
      Parameters.Add(new ValueParameter<MultiAnalyzer>("Analyzer", "The operator used to analyze each generation.", new MultiAnalyzer()));
      
      RandomCreator randomCreator = new RandomCreator();
      SolutionsCreator solutionsCreator = new SolutionsCreator();
      OffspringSelectionGeneticAlgorithmMainLoop mainLoop = new OffspringSelectionGeneticAlgorithmMainLoop();
      OperatorGraph.InitialOperator = randomCreator;

      randomCreator.RandomParameter.ActualName = "Random";
      randomCreator.SeedParameter.ActualName = SeedParameter.Name;
      randomCreator.SeedParameter.Value = null;
      randomCreator.SetSeedRandomlyParameter.ActualName = SetSeedRandomlyParameter.Name;
      randomCreator.SetSeedRandomlyParameter.Value = null;
      randomCreator.Successor = solutionsCreator;

      solutionsCreator.NumberOfSolutionsParameter.ActualName = PopulationSizeParameter.Name;
      solutionsCreator.Successor = mainLoop;

      mainLoop.AnalyzerParameter.ActualName = AnalyzerParameter.Name;
      mainLoop.ComparisonFactorModifierParameter.ActualName = ComparisonFactorModifierParameter.Name;
      mainLoop.ComparisonFactorParameter.ActualName = "ComparisonFactor";
      mainLoop.ComparisonFactorStartParameter.ActualName = ComparisonFactorLowerBoundParameter.Name;
      mainLoop.CrossoverParameter.ActualName = CrossoverParameter.Name;
      mainLoop.ElitesParameter.ActualName = ElitesParameter.Name;
      mainLoop.MaximumGenerationsParameter.ActualName = MaximumGenerationsParameter.Name;
      mainLoop.MaximumSelectionPressureParameter.ActualName = MaximumSelectionPressureParameter.Name;
      mainLoop.MutationProbabilityParameter.ActualName = MutationProbabilityParameter.Name;
      mainLoop.MutatorParameter.ActualName = MutatorParameter.Name;
      mainLoop.OffspringSelectionBeforeMutationParameter.ActualName = OffspringSelectionBeforeMutationParameter.Name;
      mainLoop.RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
      mainLoop.ResultsParameter.ActualName = "Results";
      mainLoop.SelectorParameter.ActualName = SelectorParameter.Name;
      mainLoop.SuccessRatioParameter.ActualName = SuccessRatioParameter.Name;

      foreach (ISelector selector in ApplicationManager.Manager.GetInstances<ISelector>().Where(x => !(x is IMultiObjectiveSelector)).OrderBy(x => x.Name))
        SelectorParameter.ValidValues.Add(selector);
      ISelector proportionalSelector = SelectorParameter.ValidValues.FirstOrDefault(x => x.GetType().Name.Equals("ProportionalSelector"));
      if (proportionalSelector != null) SelectorParameter.Value = proportionalSelector;
      ParameterizeSelectors();

      foreach (IDiscreteDoubleValueModifier modifier in ApplicationManager.Manager.GetInstances<IDiscreteDoubleValueModifier>().OrderBy(x => x.Name))
        ComparisonFactorModifierParameter.ValidValues.Add(modifier);
      IDiscreteDoubleValueModifier linearModifier = ComparisonFactorModifierParameter.ValidValues.FirstOrDefault(x => x.GetType().Name.Equals("LinearDiscreteDoubleValueModifier"));
      if (linearModifier != null) ComparisonFactorModifierParameter.Value = linearModifier;
      ParameterizeComparisonFactorModifiers();

      qualityAnalyzer = new BestAverageWorstQualityAnalyzer();
      selectionPressureAnalyzer = new ValueAnalyzer();
      ParameterizeAnalyzers();
      UpdateAnalyzers();

      Initialize();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      OffspringSelectionGeneticAlgorithm clone = (OffspringSelectionGeneticAlgorithm)base.Clone(cloner);
      clone.qualityAnalyzer = (BestAverageWorstQualityAnalyzer)cloner.Clone(qualityAnalyzer);
      clone.selectionPressureAnalyzer = (ValueAnalyzer)cloner.Clone(selectionPressureAnalyzer);
      clone.Initialize();
      return clone;
    }

    public override void Prepare() {
      if (Problem != null) base.Prepare();
    }

    #region Events
    protected override void OnProblemChanged() {
      ParameterizeStochasticOperator(Problem.SolutionCreator);
      ParameterizeStochasticOperator(Problem.Evaluator);
      foreach (IOperator op in Problem.Operators) ParameterizeStochasticOperator(op);
      ParameterizeSolutionsCreator();
      ParameterizMainLoop();
      ParameterizeSelectors();
      ParameterizeAnalyzers();
      UpdateCrossovers();
      UpdateMutators();
      UpdateAnalyzers();
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
      ParameterizMainLoop();
      ParameterizeSelectors();
      ParameterizeAnalyzers();
      Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
      base.Problem_EvaluatorChanged(sender, e);
    }
    protected override void Problem_OperatorsChanged(object sender, EventArgs e) {
      foreach (IOperator op in Problem.Operators) ParameterizeStochasticOperator(op);
      UpdateCrossovers();
      UpdateMutators();
      UpdateAnalyzers();
      base.Problem_OperatorsChanged(sender, e);
    }
    private void ElitesParameter_ValueChanged(object sender, EventArgs e) {
      Elites.ValueChanged += new EventHandler(Elites_ValueChanged);
      ParameterizeSelectors();
    }
    private void Elites_ValueChanged(object sender, EventArgs e) {
      ParameterizeSelectors();
    }
    private void PopulationSizeParameter_ValueChanged(object sender, EventArgs e) {
      PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
      ParameterizeSelectors();
    }
    private void PopulationSize_ValueChanged(object sender, EventArgs e) {
      ParameterizeSelectors();
    }
    private void Evaluator_QualityParameter_ActualNameChanged(object sender, EventArgs e) {
      ParameterizMainLoop();
      ParameterizeSelectors();
      ParameterizeAnalyzers();
    }
    #endregion

    #region Helpers
    [StorableHook(HookType.AfterDeserialization)]
    private void Initialize() {
      PopulationSizeParameter.ValueChanged += new EventHandler(PopulationSizeParameter_ValueChanged);
      PopulationSize.ValueChanged += new EventHandler(PopulationSize_ValueChanged);
      ElitesParameter.ValueChanged += new EventHandler(ElitesParameter_ValueChanged);
      Elites.ValueChanged += new EventHandler(Elites_ValueChanged);
      if (Problem != null) {
        Problem.Evaluator.QualityParameter.ActualNameChanged += new EventHandler(Evaluator_QualityParameter_ActualNameChanged);
      }
    }
    private void ParameterizeSolutionsCreator() {
      SolutionsCreator.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
      SolutionsCreator.SolutionCreatorParameter.ActualName = Problem.SolutionCreatorParameter.Name;
    }
    private void ParameterizMainLoop() {
      MainLoop.EvaluatorParameter.ActualName = Problem.EvaluatorParameter.Name;
      MainLoop.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
      MainLoop.QualityParameter.ActualName = Problem.Evaluator.QualityParameter.ActualName;
    }
    private void ParameterizeStochasticOperator(IOperator op) {
      if (op is IStochasticOperator)
        ((IStochasticOperator)op).RandomParameter.ActualName = RandomCreator.RandomParameter.ActualName;
    }
    private void ParameterizeSelectors() {
      foreach (ISelector selector in SelectorParameter.ValidValues) {
        selector.CopySelected = new BoolValue(true);
        selector.NumberOfSelectedSubScopesParameter.Value = null;
        selector.NumberOfSelectedSubScopesParameter.ActualName = SelectedParentsParameter.Name;
        ParameterizeStochasticOperator(selector);
      }
      if (Problem != null) {
        foreach (ISingleObjectiveSelector selector in SelectorParameter.ValidValues.OfType<ISingleObjectiveSelector>()) {
          selector.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
          selector.QualityParameter.ActualName = Problem.Evaluator.QualityParameter.ActualName;
        }
      }
    }
    private void ParameterizeAnalyzers() {
      qualityAnalyzer.ResultsParameter.ActualName = "Results";
      selectionPressureAnalyzer.Name = "SelectionPressure Analyzer";
      selectionPressureAnalyzer.ResultsParameter.ActualName = "Results";
      selectionPressureAnalyzer.ValueParameter.ActualName = "SelectionPressure";
      selectionPressureAnalyzer.ValueParameter.Depth = 0;
      selectionPressureAnalyzer.ValuesParameter.ActualName = "Selection Pressure History";
      if (Problem != null) {
        qualityAnalyzer.MaximizationParameter.ActualName = Problem.MaximizationParameter.Name;
        qualityAnalyzer.QualityParameter.ActualName = Problem.Evaluator.QualityParameter.ActualName;
        qualityAnalyzer.BestKnownQualityParameter.ActualName = Problem.BestKnownQualityParameter.Name;
      }
    }
    private void ParameterizeComparisonFactorModifiers() {
      foreach (IDiscreteDoubleValueModifier modifier in ComparisonFactorModifierParameter.ValidValues) {
        modifier.IndexParameter.ActualName = "Generations";
        modifier.EndIndexParameter.ActualName = MaximumGenerationsParameter.Name;
        modifier.EndValueParameter.ActualName = ComparisonFactorUpperBoundParameter.Name;
        modifier.StartIndexParameter.Value = new IntValue(0);
        modifier.StartValueParameter.ActualName = ComparisonFactorLowerBoundParameter.Name;
        modifier.ValueParameter.ActualName = "ComparisonFactor";
      }
    }
    private void UpdateCrossovers() {
      ICrossover oldCrossover = CrossoverParameter.Value;
      CrossoverParameter.ValidValues.Clear();
      foreach (ICrossover crossover in Problem.Operators.OfType<ICrossover>().OrderBy(x => x.Name))
        CrossoverParameter.ValidValues.Add(crossover);
      if (oldCrossover != null) {
        ICrossover crossover = CrossoverParameter.ValidValues.FirstOrDefault(x => x.GetType() == oldCrossover.GetType());
        if (crossover != null) CrossoverParameter.Value = crossover;
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
    private void UpdateAnalyzers() {
      Analyzer.Operators.Clear();
      Analyzer.Operators.Add(qualityAnalyzer);
      Analyzer.Operators.Add(selectionPressureAnalyzer);
      if (Problem != null) {
        foreach (IAnalyzer analyzer in Problem.Operators.OfType<IAnalyzer>().OrderBy(x => x.Name)) {
          foreach (IScopeTreeLookupParameter param in analyzer.Parameters.OfType<IScopeTreeLookupParameter>())
            param.Depth = 1;
          Analyzer.Operators.Add(analyzer);
        }
      }
    }
    #endregion
  }
}
