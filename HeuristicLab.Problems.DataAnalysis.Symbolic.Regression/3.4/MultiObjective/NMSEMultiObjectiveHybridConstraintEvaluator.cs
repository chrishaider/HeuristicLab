using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.DataAnalysis.Symbolicr;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [Item("Hybrid Evalutor", "Hybrid Evalutor")]
  [StorableType("DF3D4D23-09B7-4B13-B0EB-96A0B1DCB751")]
  public class NMSEMultiObjectiveHybridConstraintEvaluator : SymbolicRegressionMultiObjectiveEvaluator, IMultiObjectiveConstraintsEvaluator {
    private const string NumConstraintsParameterName = "NumConstraints";
    private const string ConstraintEstimatorParameterName = "ConstraintEstimator";
    private const string UseSimplificationParameterName = "UseSimplification";
    private const string MaximumGenerationsParameterName = "MaximumGenerations";
    private const string GenerationsParameterName = "Generations";
    private const string SamplesParameterName = "Samples";

    #region Parameters
    public IFixedValueParameter<IntValue> NumConstraintsParameter =>
      (IFixedValueParameter<IntValue>)Parameters[NumConstraintsParameterName];

    public IValueParameter<IConstraintEstimator> ConstraintEstimatorParameter =>
      (IValueParameter<IConstraintEstimator>)Parameters[ConstraintEstimatorParameterName];

    public IFixedValueParameter<BoolValue> UseSimplificationParameter =>
      (IFixedValueParameter<BoolValue>)Parameters[UseSimplificationParameterName];

    public IValueLookupParameter<StringValue> SamplesParameter =>
      (IValueLookupParameter<StringValue>)Parameters[SamplesParameterName];

    public int NumConstraints {
      get => NumConstraintsParameter.Value.Value;
      set => NumConstraintsParameter.Value.Value = value;
    }

    public bool UseSimplification {
      get => UseSimplificationParameter.Value.Value;
      set => UseSimplificationParameter.Value.Value = value;
    }

    public IConstraintEstimator ConstraintEstimator {
      get => ConstraintEstimatorParameter.Value;
      set => ConstraintEstimatorParameter.Value = value;
    }

    public string Samples {
      get => SamplesParameter.Value.Value;
      set => SamplesParameter.Value.Value = value;
    }

    public override IEnumerable<bool> Maximization => new bool[2]; // minimize all objectives


    public ILookupParameter<IntValue> MaximumGenerationsParameter => (ILookupParameter<IntValue>)Parameters[MaximumGenerationsParameterName];

    public ILookupParameter<IntValue> GenerationsParameter =>
      (ILookupParameter<IntValue>)Parameters[GenerationsParameterName];
    #endregion

    #region Constructors

    public NMSEMultiObjectiveHybridConstraintEvaluator() {
      Parameters.Add(new FixedValueParameter<IntValue>(NumConstraintsParameterName, new IntValue(0)));
      Parameters.Add(new ValueParameter<IConstraintEstimator>(ConstraintEstimatorParameterName, new IntervalArithPessimisticEstimator()));
      Parameters.Add(new FixedValueParameter<BoolValue>(UseSimplificationParameterName, new BoolValue(false)));

      Parameters.Add(new LookupParameter<IntValue>(MaximumGenerationsParameterName, "The maximum number of generations which should be processed."));
      Parameters.Add(new LookupParameter<IntValue>(GenerationsParameterName, "The current number of generations."));
      Parameters.Add(new ValueLookupParameter<StringValue>(SamplesParameterName, "Holds the samples."));
    }

    [StorableConstructor]
    protected NMSEMultiObjectiveHybridConstraintEvaluator(StorableConstructorFlag _) : base(_) { }

    protected NMSEMultiObjectiveHybridConstraintEvaluator(NMSEMultiObjectiveHybridConstraintEvaluator original, Cloner cloner) : base(original, cloner) { }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      if(!Parameters.ContainsKey(UseSimplificationParameterName)) {
        Parameters.Add(new FixedValueParameter<BoolValue>(UseSimplificationParameterName, new BoolValue(false)));
      }
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new NMSEMultiObjectiveHybridConstraintEvaluator(this, cloner);
    }

    #endregion

    public override IOperation InstrumentedApply() {
      var rows = GenerateRowsToEvaluate();
      var tree = SymbolicExpressionTreeParameter.ActualValue;
      var problemData = ProblemDataParameter.ActualValue;
      var interpreter = SymbolicDataAnalysisTreeInterpreterParameter.ActualValue;
      var estimationLimits = EstimationLimitsParameter.ActualValue;
      var applyLinearScaling = ApplyLinearScalingParameter.ActualValue.Value;
      var useSimplification = UseSimplificationParameter.ActualValue;

      if(ConstraintEstimator is SamplingEsitmator samplingEstimator) {
        if(Samples == null) {
          SamplesParameter.ActualValue = new StringValue("ASDF");
        }

        
      }

      if(UseParameterOptimization) {
        SymbolicRegressionParameterOptimizationEvaluator.OptimizeParameters(interpreter, tree, problemData, rows,
          false,
          ParameterOptimizationIterations,
          ParameterOptimizationUpdateVariableWeights,
          estimationLimits.Lower,
          estimationLimits.Upper);
      } else {
        if(applyLinearScaling) {
          var rootNode = new ProgramRootSymbol().CreateTreeNode();
          var startNode = new StartSymbol().CreateTreeNode();
          var offset = tree.Root.GetSubtree(0) //Start
                                .GetSubtree(0); //Offset
          var scaling = offset.GetSubtree(0);

          // Check if tree contains offset and scaling nodes
          if(offset.Symbol is not Addition || scaling.Symbol is not Multiplication)
            throw new ArgumentException($"{ItemName} can only be used with LinearScalingGrammar.");


          var t = (ISymbolicExpressionTreeNode)scaling.GetSubtree(0).Clone();
          rootNode.AddSubtree(startNode);
          startNode.AddSubtree(t);
          var newTree = new SymbolicExpressionTree(rootNode);

          // calculate alpha and beta for scaling
          var estimatedValues = interpreter.GetSymbolicExpressionTreeValues(newTree, problemData.Dataset, rows);

          var targetValues = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, rows);
          OnlineLinearScalingParameterCalculator.Calculate(estimatedValues, targetValues, out var alpha, out var beta,
            out var errorState);
          if(errorState == OnlineCalculatorError.None) {
            // Set alpha and beta to the scaling nodes from linear scaling grammar
            var offsetParameter = offset.GetSubtree(1) as NumberTreeNode;
            offsetParameter.Value = alpha;
            var scalingParameter = scaling.GetSubtree(1) as NumberTreeNode;
            scalingParameter.Value = beta;
          }
        } // else alpha and beta are evolved
        //Do simplification after scaling is done, otherwise the scaling and offset terms may not exist
        if(UseSimplification) {
          tree = TreeSimplifier.Simplify(tree);
        }
      }
      var maxGenerations = MaximumGenerationsParameter.ActualValue.Value;
      var currentGeneration = 0;
      if(GenerationsParameter.ActualValue != null)
        currentGeneration = GenerationsParameter.ActualValue.Value;

      //Check if dependency problem can accour due to multiple variables occourences
      HasMultipleVariablesOccourences(tree);


      var qualities = Calculate(interpreter, tree, estimationLimits.Lower, estimationLimits.Upper, problemData,
        rows, ConstraintEstimator, currentGeneration, maxGenerations, DecimalPlaces);
      QualitiesParameter.ActualValue = new DoubleArray(qualities);
      return base.InstrumentedApply();
    }

    public override double[] Evaluate(
      IExecutionContext context, ISymbolicExpressionTree tree,
      IRegressionProblemData problemData,
      IEnumerable<int> rows) {
      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = context;
      EstimationLimitsParameter.ExecutionContext = context;
      ApplyLinearScalingParameter.ExecutionContext = context;

      var maxGenerations = MaximumGenerationsParameter.ActualValue.Value;
      var currentGeneration = 0;
      if(GenerationsParameter.ActualValue != null)
        currentGeneration = GenerationsParameter.ActualValue.Value;

      var quality = Calculate(SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, tree,
        EstimationLimitsParameter.ActualValue.Lower, EstimationLimitsParameter.ActualValue.Upper,
        problemData, rows, ConstraintEstimator, currentGeneration, maxGenerations, DecimalPlaces);

      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = null;
      EstimationLimitsParameter.ExecutionContext = null;
      ApplyLinearScalingParameter.ExecutionContext = null;

      return quality;
    }

    public static double[] Calculate(
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      ISymbolicExpressionTree solution, double lowerEstimationLimit,
      double upperEstimationLimit,
      IRegressionProblemData problemData, IEnumerable<int> rows, IConstraintEstimator estimator, int currentGeneration, int maxGenerations, int decimalPlaces = 4) {
      var estimatedValues = interpreter.GetSymbolicExpressionTreeValues(solution, problemData.Dataset, rows);
      var targetValues = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, rows);
      var constraints = Enumerable.Empty<ShapeConstraint>();

      var intervalCollection = problemData.VariableRanges;

      var boundedEstimatedValues = estimatedValues.LimitToRange(lowerEstimationLimit, upperEstimationLimit);
      var nmse = OnlineNormalizedMeanSquaredErrorCalculator.Calculate(targetValues, boundedEstimatedValues, out var errorState);

      if(errorState != OnlineCalculatorError.None) nmse = 1.0;

      if(decimalPlaces >= 0) {
        nmse = Math.Round(nmse, decimalPlaces);
      }

      if(nmse > 1)
        nmse = 1.0;

      var end = 100;
      var start = 0;
      var proportion = (double)(end - start) / maxGenerations;
      var penalty = (int)(start + (proportion * currentGeneration));
      var penaltyFactor = penalty / 100.0;

      var objectives = new List<double> { nmse };
      var violations = 0.0d;
      switch(estimator) {
        case IPessimisticEstimator pessimisticEstimator:
          violations = IntervalUtil.GetConstraintViolations(constraints, pessimisticEstimator, intervalCollection, solution).Sum();
          break;
        case IOptimisticEstimator optEstimator: {
            violations = IntervalUtil.GetConstraintViolations(constraints, optEstimator, problemData, solution).Sum();
            break;
          }
      }

      if(decimalPlaces >= 0) {
        violations = Math.Round(violations, decimalPlaces);
      }

      objectives.Add(double.IsNaN(violations) ? double.PositiveInfinity : violations * penaltyFactor);

      return objectives.ToArray();
    }


    private bool HasMultipleVariablesOccourences(ISymbolicExpressionTree tree) {
      var treeNodes = new Dictionary<string, int>();

      foreach(var node in tree.IterateNodesPostfix().OfType<VariableTreeNode>()) {
        if (treeNodes.ContainsKey(node.VariableName)) treeNodes[node.VariableName]++;
        else treeNodes.Add(node.VariableName, 1);


      }

      foreach(var val in treeNodes.Values) {
        if(val > 1) {
          return true;
        }
      }

      return false;
    }
  }
}

