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
using HeuristicLab.Random;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [Item("Hybrid constraint evaluator.",
    "Calculates the constraint violations in a hybrid approach.")]
  [StorableType("CB8B3FED-B971-4C29-B5AC-36541DC8FE10")]
  public class HybridConstraintEvaluator : SymbolicRegressionMultiObjectiveEvaluator, IMultiObjectiveConstraintsEvaluator {
    #region ParameterNames
    private const string NumConstraintsParameterName = "Number of Constraints";
    private const string OptimisticEstimatorParameterName = "Optimistic Estimator";
    private const string PessimisticEstimatorParameterName = "Pessimistic Estimator";
    private const string SamplesParameterName = "Samples";
    private const string SampleSizeParameterName = "SampleSize";
    #endregion
    #region Parameters
    public override IEnumerable<bool> Maximization => new bool[2];
    public IFixedValueParameter<IntValue> NumConstraintsParameter =>
      (IFixedValueParameter<IntValue>)Parameters[NumConstraintsParameterName];
    public IValueParameter<IPessimisticEstimator> PessimisticEstimatorParameter =>
      (IValueParameter<IPessimisticEstimator>)Parameters[PessimisticEstimatorParameterName];
    public IValueParameter<IPessimisticEstimator> OptimisticEstimatorParameter =>
      (IValueParameter<IPessimisticEstimator>)Parameters[OptimisticEstimatorParameterName];
    public IValueLookupParameter<Dataset> SamplesParameter =>
     (IValueLookupParameter<Dataset>)Parameters[SamplesParameterName];
    public IFixedValueParameter<IntValue> SampleSizeParameter =>
      (IFixedValueParameter<IntValue>)Parameters[SampleSizeParameterName];


    public IPessimisticEstimator PessimisticEstimator {
      get => PessimisticEstimatorParameter.Value;
      set => PessimisticEstimator = value;
    }

    public IPessimisticEstimator OptimisticEstimator {
      get => OptimisticEstimatorParameter.Value;
      set => OptimisticEstimator = value;
    }

    public int SampleSize {
      get => SampleSizeParameter.Value.Value;
      set => SampleSizeParameter.Value.Value = value;
    }

    public Dataset Samples {
      get => SamplesParameter.Value;
      set => SamplesParameter.Value = value;
    }
    #endregion
    #region Constructors
    public HybridConstraintEvaluator() : base () { 
      Parameters.Add(new FixedValueParameter<IntValue>(NumConstraintsParameterName, new IntValue(2)));
      Parameters.Add(new ValueParameter<IPessimisticEstimator>(PessimisticEstimatorParameterName, "Estimator used for the pessimistic esitmation.", new IntervalArithPessimisticEstimator()));
      Parameters.Add(new ValueParameter<IPessimisticEstimator>(OptimisticEstimatorParameterName, "Estimator used for the optimistic esitmation.", new SamplingEsitmator()));
      Parameters.Add(new ValueLookupParameter<Dataset>(SamplesParameterName, "Holds the samples."));
      Parameters.Add(new FixedValueParameter<IntValue>(SampleSizeParameterName, "Sets the amount of samples taken.", new IntValue(10000)));
      NumConstraintsParameter.Hidden = true;
    }
    [StorableConstructor]
    protected HybridConstraintEvaluator(StorableConstructorFlag _) : base(_) { }
    protected HybridConstraintEvaluator(HybridConstraintEvaluator original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new HybridConstraintEvaluator(this, cloner);
    }
    #endregion

    public override IOperation InstrumentedApply() {
      var rows = GenerateRowsToEvaluate();
      var tree = SymbolicExpressionTreeParameter.ActualValue;
      var problemData = ProblemDataParameter.ActualValue;
      var interpreter = SymbolicDataAnalysisTreeInterpreterParameter.ActualValue;
      var estimationLimits = EstimationLimitsParameter.ActualValue;
      var applyLinearScaling = ApplyLinearScalingParameter.ActualValue.Value;
      var variableRanges = problemData.VariableRanges;
      var random = new MersenneTwister();
      var pessimistic = true;

      if(SamplesParameter.ActualValue == null) {
        if (OptimisticEstimator is SamplingEsitmator samplingEsitmator) {
          var data = new List<List<double>>();
          foreach(var varaiable in problemData.AllowedInputVariables) {
            var values = new List<double>();
            var sampleInterval = variableRanges.GetInterval(varaiable);
            for(var i = 0; i < SampleSize; ++i) {
              var value = new UniformDistributedRandom(random, sampleInterval.LowerBound, sampleInterval.UpperBound).NextDouble();
              values.Add(value);
            }
            data.Add(values);
          }
          var dataset = new Dataset(problemData.AllowedInputVariables, data);
          SamplesParameter.Value = dataset;
          samplingEsitmator.Samples = SamplesParameter.ActualValue;
        }
      }

      if(applyLinearScaling) {
        CalcLinearScalingTerms(tree, problemData, rows, interpreter);
      }
      if(MultipleVariableOccurrence(tree)) {
        pessimistic = false;
      }

      

      var quality = Calculate(
        interpreter,
        tree,
        estimationLimits.Lower,
        estimationLimits.Upper,
        problemData,
        rows,
        PessimisticEstimator,
        OptimisticEstimator,
        pessimistic);

      QualitiesParameter.ActualValue = new DoubleArray(quality);

      return base.InstrumentedApply();
    }

    public override void InitializeState() {
      SamplesParameter.Value = null;

      base.InitializeState();
    }

    #region Evaluation
    public override double[] Evaluate(
      IExecutionContext context, 
      ISymbolicExpressionTree tree, 
      IRegressionProblemData problemData, 
      IEnumerable<int> rows) {
      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = context;
      EstimationLimitsParameter.ExecutionContext = context;
      ApplyLinearScalingParameter.ExecutionContext = context;

      var quality = Calculate(
        SymbolicDataAnalysisTreeInterpreterParameter.ActualValue,
        tree,
        EstimationLimitsParameter.ActualValue.Lower,
        EstimationLimitsParameter.ActualValue.Upper,
        problemData,
        rows,
        PessimisticEstimator,
        OptimisticEstimator
        );

      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = null;
      EstimationLimitsParameter.ExecutionContext = null;
      ApplyLinearScalingParameter.ExecutionContext = null;

      return quality;
    }

    public static double[] Calculate(
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      ISymbolicExpressionTree solution,
      double lowerEstimationLimit,
      double upperEsimationLimit,
      IRegressionProblemData problemData,
      IEnumerable<int> rows,
      IPessimisticEstimator pessimisitcEstimator,
      IPessimisticEstimator optimisticEstimator,
      bool pessimistic = true,
      int decimalPlaces = 4) {

      var estimatedValues = interpreter.GetSymbolicExpressionTreeValues(solution, problemData.Dataset, rows);
      var boundedEstimatedValues = estimatedValues.LimitToRange(lowerEstimationLimit, upperEsimationLimit);
      var targetValues = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, rows);

      var nmse = OnlineNormalizedMeanSquaredErrorCalculator.Calculate(targetValues, boundedEstimatedValues, out var errorState);
      if(errorState != OnlineCalculatorError.None)
        nmse = 1.0;

      var constraints = Enumerable.Empty<ShapeConstraint>();
      if (problemData is ShapeConstrainedRegressionProblemData scProblemData) {
        constraints = scProblemData.ShapeConstraints.EnabledConstraints;
      }
      var intervalCollection = problemData.VariableRanges;
      var violations = IntervalUtil.GetConstraintViolations(constraints, pessimistic ? pessimisitcEstimator : optimisticEstimator, intervalCollection, solution).Sum();
      if (double.IsNaN(violations) || double.IsInfinity(violations)) {
        violations = double.MaxValue;
      }

      return new double[2] { nmse, Math.Round(violations, decimalPlaces) };
    }

    private static bool MultipleVariableOccurrence(ISymbolicExpressionTree solution) {
      var occurrences = new List<string>();

      foreach(var node in solution.IterateNodesPostfix().OfType<VariableTreeNode>()) {
        if(occurrences.Contains(node.VariableName)) return true;
        occurrences.Add(node.VariableName);
      }

      return false;
    }
    #endregion


    #region Scaling & Optimize
    private static void CalcLinearScalingTerms(
      ISymbolicExpressionTree tree,
      IRegressionProblemData problemData,
      IEnumerable<int> rows,
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter) {
      var rootNode = new ProgramRootSymbol().CreateTreeNode();
      var startNode = new StartSymbol().CreateTreeNode();
      var offset = tree.Root.GetSubtree(0) //Start
                            .GetSubtree(0); //Offset
      var scaling = offset.GetSubtree(0);

      //Check if tree contains offset and scaling nodes
      if(!(offset.Symbol is Addition) || !(scaling.Symbol is Multiplication))
        throw new ArgumentException($"Scaling can only be used with LinearScalingGrammar.");

      var t = (ISymbolicExpressionTreeNode)scaling.GetSubtree(0).Clone();
      rootNode.AddSubtree(startNode);
      startNode.AddSubtree(t);
      var newTree = new SymbolicExpressionTree(rootNode);

      //calculate alpha and beta for scaling
      var estimatedValues = interpreter.GetSymbolicExpressionTreeValues(newTree, problemData.Dataset, rows);

      var targetValues = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, rows);
      OnlineLinearScalingParameterCalculator.Calculate(estimatedValues, targetValues, out var alpha, out var beta,
        out var errorState);

      if(errorState == OnlineCalculatorError.None) {
        //Set alpha and beta to the scaling nodes from grammar
        var offsetParameter = offset.GetSubtree(1) as NumberTreeNode;
        offsetParameter.Value = alpha;
        var scalingParameter = scaling.GetSubtree(1) as NumberTreeNode;
        scalingParameter.Value = beta;
      }
    }
    #endregion
  }
}
