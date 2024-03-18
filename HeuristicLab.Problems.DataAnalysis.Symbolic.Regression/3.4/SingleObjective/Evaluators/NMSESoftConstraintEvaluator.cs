﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Random;
using static alglib;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [Item("NMSE soft constraints evaluator", "Calculates NMSE of a symbolic regression solution and checks constraints. The fitness is a combination of NMSE and constraint violations.")]
  [StorableType("0A5E811A-BB3C-457A-93B5-EE8938D77F01")]
  public class NMSESoftConstraintEvaluator : SymbolicRegressionSingleObjectiveEvaluator {
    #region Parameter/Properties

    private const string OptimizeParametersParameterName = "OptimizeParameters";
    private const string ParameterOptimizationIterationsParameterName = "ParameterOptimizationIterations";
    private const string UseSoftConstraintsParameterName = "UseSoftConstraintsEvaluation";
    private const string BoundsEstimatorParameterName = "ConstraintEstimator";
    private const string PenaltyFactorParameterName = "PenaltyFactor";
    private const string MaximumGenerationsParameterName = "MaximumGenerations";
    private const string GenerationsParameterName = "Generations";


    private const string SamplesParameterName = "Samples";
    private const string SampleSizeParameterName = "SampleSize";

    private static int genChanged = 0;
    private static int end = 100;
    private static int start = 0;
    private static double maxTemperature = 1.0; // Adjust based on your preference
    private static double minTemperature = 0.1; // Adjust based on your preference
    private static double coolingRate = 0.95; // Adjust based on your preference


    public IFixedValueParameter<BoolValue> OptimizerParametersParameter =>
      (IFixedValueParameter<BoolValue>)Parameters[OptimizeParametersParameterName];

    public IFixedValueParameter<IntValue> ParameterOptimizationIterationsParameter =>
      (IFixedValueParameter<IntValue>)Parameters[ParameterOptimizationIterationsParameterName];

    public IFixedValueParameter<BoolValue> UseSoftConstraintsParameter =>
      (IFixedValueParameter<BoolValue>)Parameters[UseSoftConstraintsParameterName];

    public IValueParameter<IPessimisticEstimator> BoundsEstimatorParameter =>
      (IValueParameter<IPessimisticEstimator>)Parameters[BoundsEstimatorParameterName];
    public IFixedValueParameter<DoubleValue> PenaltyFactorParameter =>
      (IFixedValueParameter<DoubleValue>)Parameters[PenaltyFactorParameterName];

    public IValueLookupParameter<Dataset> SamplesParameter =>
      (IValueLookupParameter<Dataset>)Parameters[SamplesParameterName];

    public IFixedValueParameter<IntValue> SampleSizeParameter =>
      (IFixedValueParameter<IntValue>)Parameters[SampleSizeParameterName];

    public bool OptimizeParameters {
      get => OptimizerParametersParameter.Value.Value;
      set => OptimizerParametersParameter.Value.Value = value;
    }

    public int ParameterOptimizationIterations {
      get => ParameterOptimizationIterationsParameter.Value.Value;
      set => ParameterOptimizationIterationsParameter.Value.Value = value;
    }

    public bool UseSoftConstraints {
      get => UseSoftConstraintsParameter.Value.Value;
      set => UseSoftConstraintsParameter.Value.Value = value;
    }

    public IPessimisticEstimator PessimisticEstimator {
      get => BoundsEstimatorParameter.Value;
      set => BoundsEstimatorParameter.Value = value;
    }

    public double PenalityFactor {
      get => PenaltyFactorParameter.Value.Value;
      set => PenaltyFactorParameter.Value.Value = value;
    }

    public Dataset Samples {
      get => SamplesParameter.ActualValue;
      set => SamplesParameter.ActualValue = value;
    }

    public int SampleSize {
      get => SampleSizeParameter.Value.Value;
      set => SampleSizeParameter.Value.Value = value;
    }


    public override bool Maximization => false; // NMSE is minimized

    public ILookupParameter<IntValue> MaximumGenerationsParameter => (ILookupParameter<IntValue>)Parameters[MaximumGenerationsParameterName];

    public ILookupParameter<IntValue> GenerationsParameter =>
      (ILookupParameter<IntValue>)Parameters[GenerationsParameterName];

    #endregion

    #region Constructors/Cloning

    [StorableConstructor]
    protected NMSESoftConstraintEvaluator(StorableConstructorFlag _) : base(_) { }

    protected NMSESoftConstraintEvaluator(
      NMSESoftConstraintEvaluator original, Cloner cloner) : base(original, cloner) { }

    public NMSESoftConstraintEvaluator() {
      Parameters.Add(new FixedValueParameter<BoolValue>(OptimizeParametersParameterName,
        "Define whether optimization of parameters is active or not (default: false).", new BoolValue(false)));
      Parameters.Add(new FixedValueParameter<IntValue>(ParameterOptimizationIterationsParameterName,
        "Define how many parameter optimization steps should be performed (default: 10).", new IntValue(10)));
      Parameters.Add(new FixedValueParameter<BoolValue>(UseSoftConstraintsParameterName,
        "Define whether the constraints are penalized by soft or hard constraints (default: false).", new BoolValue(false)));
      Parameters.Add(new ValueParameter<IPessimisticEstimator>(BoundsEstimatorParameterName,
        "The estimator which is used to estimate output ranges of models (default: interval arithmetic).", new IntervalArithPessimisticEstimator()));
      Parameters.Add(new FixedValueParameter<DoubleValue>(PenaltyFactorParameterName,
        "Punishment factor for constraint violations for soft constraint handling (fitness = NMSE + penaltyFactor * avg(violations)) (default: 1.0)", new DoubleValue(1.0)));

      Parameters.Add(new LookupParameter<IntValue>(MaximumGenerationsParameterName, "The maximum number of generations which should be processed."));
      Parameters.Add(new LookupParameter<IntValue>(GenerationsParameterName, "The current number of generations."));

      Parameters.Add(new ValueLookupParameter<Dataset>(SamplesParameterName, "Holds the samples."));
      Parameters.Add(new FixedValueParameter<IntValue>(SampleSizeParameterName,
        "Sets the amount of samples taken.", new IntValue(10000)));
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new NMSESoftConstraintEvaluator(this, cloner);
    }

    public override void InitializeState() {
      base.InitializeState();

      SamplesParameter.Value = null;
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

      if (PessimisticEstimator is SamplingEsitmator samplingEstimator) {
        if (SamplesParameter.ActualValue == null) {
          //Generate my samples
          var data = new List<List<double>>();
          foreach (var variable in problemData.AllowedInputVariables) {
            var values = new List<double>();
            var sampleInterval = variableRanges.GetInterval(variable);
            for (var i = 0; i < SampleSize; ++i) {
              var value = new UniformDistributedRandom(random, sampleInterval.LowerBound, sampleInterval.UpperBound).NextDouble();
              values.Add(value);
            }
            //Add min and max to samples
            values.Add(sampleInterval.LowerBound);
            values.Add(sampleInterval.UpperBound);
            data.Add(values);
          }
          var dataset = new Dataset(problemData.AllowedInputVariables, data);
          SamplesParameter.Value = dataset;
        }

        samplingEstimator.Samples = SamplesParameter.ActualValue;
      }


      var quality = Evaluate(tree, problemData, rows, interpreter, applyLinearScaling, estimationLimits.Lower, estimationLimits.Upper);
      QualityParameter.ActualValue = new DoubleValue(quality);

      return base.InstrumentedApply();
    }


    public static double Calculate(
      ISymbolicExpressionTree tree,
      IRegressionProblemData problemData, IEnumerable<int> rows,
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      double lowerEstimationLimit, double upperEstimationLimit,
      IPessimisticEstimator estimator,
      int maxGenerations, int currentGeneration) {


      var constraints = Enumerable.Empty<ShapeConstraint>();
      if (problemData is ShapeConstrainedRegressionProblemData scProbData)
        constraints = scProbData.ShapeConstraints.EnabledConstraints;

      var estimatedValues = interpreter.GetSymbolicExpressionTreeValues(tree, problemData.Dataset, rows);
      var boundedEstimatedValues = estimatedValues.LimitToRange(lowerEstimationLimit, upperEstimationLimit);
      var targetValues = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, rows);
      var nmse = OnlineNormalizedMeanSquaredErrorCalculator.Calculate(targetValues, boundedEstimatedValues, out var errorState);
      if (errorState != OnlineCalculatorError.None)
        return 1.0;

      if (!constraints.Any())
        return nmse;

      var intervalCollection = problemData.VariableRanges;
      var constraintViolations = IntervalUtil.GetConstraintViolations(constraints, estimator, intervalCollection, tree);

      // infinite/NaN constraints
      if (constraintViolations.Any(x => double.IsNaN(x) || double.IsInfinity(x)))
        return 1.0;

      // soft constraints
      //Calculate penalty factor as a percentage of current generation
      //Linear approach
      //var end = 100;
      //var start = 50;
      //var proportion = (double)(end - start) / maxGenerations;
      //var penalty = (int)(start + (proportion * currentGeneration));
      //var penaltyFactor = penalty / 100.0;
      //var weightedViolationsAvg = constraints
      //  .Zip(constraintViolations, (c, v) => c.Weight * v)
      //  .Average();

      //var violationProp = 1.0 / constraints.Count();
      //var weightedViolationsAvg = constraintViolations.Where(cv => cv > 0).Aggregate(0.0, (current, cv) => current + violationProp);

      //Exponential approach
      //var end = 100;
      //var start = 20;
      //var exponent = 2.0; 
      //var penalty = (int)(start + Math.Pow(currentGeneration / (double)maxGenerations, exponent) * (end - start));
      //var penaltyFactor = penalty / 100.0;
      //var weightedViolationsAvg = constraints.Zip(constraintViolations, (c, v) => c.Weight * v).Average();

      //Simulated


      // Calculate the current temperature based on the progress of generations
      //var currentTemperature = maxTemperature * Math.Pow(coolingRate, currentGeneration);

      // Calculate the penalty using the temperature
      //var penalty = (int)(start + (currentTemperature / maxTemperature) * (end - start));
      //var penaltyFactor = penalty / 100.0;

      var penaltyFactor = GetAdaptiveWeight(currentGeneration, maxGenerations, 0.4, 0.95);


      var weightedViolationsAvg = constraints.Zip(constraintViolations, (c, v) => c.Weight * v).Average();

      var error= nmse + penaltyFactor * weightedViolationsAvg;
      //if(genChanged < currentGeneration)
      //  AdaptParameters(currentGeneration, maxGenerations, ref maxTemperature, ref minTemperature, ref coolingRate);


      return Math.Min(error, 1.0);
    }

    public override double Evaluate(
      IExecutionContext context, ISymbolicExpressionTree tree, IRegressionProblemData problemData,
      IEnumerable<int> rows) {
      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = context;
      EstimationLimitsParameter.ExecutionContext = context;


      ApplyLinearScalingParameter.ExecutionContext = context;

      var maxGenerations = MaximumGenerationsParameter.ActualValue.Value;
      var currentGeneration = 0;
      if (GenerationsParameter.ActualValue != null)
        currentGeneration = GenerationsParameter.ActualValue.Value;

      var nmse = Calculate(
        tree, problemData, rows,
        SymbolicDataAnalysisTreeInterpreterParameter.ActualValue,
        EstimationLimitsParameter.ActualValue.Lower,
        EstimationLimitsParameter.ActualValue.Upper,
        PessimisticEstimator,
        maxGenerations, currentGeneration);

      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = null;
      EstimationLimitsParameter.ExecutionContext = null;
      ApplyLinearScalingParameter.ExecutionContext = null;

      return nmse;
    }

    static double GetAdaptiveWeight(int currentGeneration, int maxGenerations, double cooldownProbability, double cooldownFactor) {
      double weight = 0.0;
      bool cooldown = false;

      if (!cooldown) {
        weight = (double)currentGeneration / (maxGenerations - 1);
      } else {
        weight *= cooldownFactor;
      }
      var random = new MersenneTwister();
      // Check for cooldown phase
      if (random.NextDouble() < cooldownProbability) {
        cooldown = true;
      } else {
        cooldown = false;
      }

      return weight;
    }

    private static void AdaptParameters(int currentGeneration, int maxIterations, ref double maxTemp, ref double minTemp,
      ref double coolingRate) {
      if (currentGeneration % (maxIterations / 100) != 0) return;
      maxTemp *= 0.9;
      minTemp *= 1.1;
      coolingRate *= 0.98;
      genChanged++;
    }


    public override double Evaluate(
      ISymbolicExpressionTree tree,
      IRegressionProblemData problemData,
      IEnumerable<int> rows,
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      bool applyLinearScaling = true,
      double lowerEstimationLimit = double.MinValue,
      double upperEstimationLimit = double.MaxValue) {

      var maxGenerations = MaximumGenerationsParameter.ActualValue.Value;
      var currentGeneration = 0;
      if (GenerationsParameter.ActualValue != null) 
        currentGeneration = GenerationsParameter.ActualValue.Value;

      if (OptimizeParameters)
        Optimize(
          interpreter, tree,
          problemData, rows,
          ParameterOptimizationIterations,
          updateVariableWeights: true,
          lowerEstimationLimit,
          upperEstimationLimit);
      else if (applyLinearScaling) // extra scaling terms, which are included in tree
        CalcLinearScalingTerms(tree, problemData, rows, interpreter);

      return Calculate(
        tree, problemData,
        rows, interpreter,
        lowerEstimationLimit,
        upperEstimationLimit,
        PessimisticEstimator,
        maxGenerations,
        currentGeneration);
    }

    #region Linear Scaling

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
      if (!(offset.Symbol is Addition) || !(scaling.Symbol is Multiplication))
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

      if (errorState == OnlineCalculatorError.None) {
        //Set alpha and beta to the scaling nodes from grammar
        var offsetParameter = offset.GetSubtree(1) as NumberTreeNode;
        offsetParameter.Value = alpha;
        var scalingParameter = scaling.GetSubtree(1) as NumberTreeNode;
        scalingParameter.Value = beta;
      }
    }


    #endregion

    #region Parameter Optimization

    public static double Optimize(ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      ISymbolicExpressionTree tree, IRegressionProblemData problemData, IEnumerable<int> rows,
      int maxIterations, bool updateVariableWeights = true,
      double lowerEstimationLimit = double.MinValue, double upperEstimationLimit = double.MaxValue,
      bool updateParametersInTree = true, Action<double[], double, object> iterationCallback = null, EvaluationsCounter counter = null) {

      // Numeric parameters in the tree become variables for parameter optimization.
      // Variables in the tree become parameters (fixed values) for parameter optimization.
      // For each parameter (variable in the original tree) we store the 
      // variable name, variable value (for factor vars) and lag as a DataForVariable object.
      // A dictionary is used to find parameters
      double[] initialParameters;
      var parameters = new List<TreeToAutoDiffTermConverter.DataForVariable>();

      TreeToAutoDiffTermConverter.ParametricFunction func;
      TreeToAutoDiffTermConverter.ParametricFunctionGradient func_grad;
      if (!TreeToAutoDiffTermConverter.TryConvertToAutoDiff(tree, updateVariableWeights, addLinearScalingTerms: false, out parameters, out initialParameters, out func, out func_grad))
        throw new NotSupportedException("Could not optimize parameters of symbolic expression tree due to not supported symbols used in the tree.");
      var parameterEntries = parameters.ToArray(); // order of entries must be the same for x

      // extract initial parameters
      double[] c = (double[])initialParameters.Clone();

      double originalQuality = SymbolicRegressionSingleObjectiveMeanSquaredErrorEvaluator.Calculate(
        tree, problemData, rows,
        interpreter, applyLinearScaling: false,
        lowerEstimationLimit,
        upperEstimationLimit);

      if (counter == null) counter = new EvaluationsCounter();
      var rowEvaluationsCounter = new EvaluationsCounter();

      alglib.minlmreport rep;

      IDataset ds = problemData.Dataset;
      int n = rows.Count();
      int k = parameters.Count;

      double[,] x = new double[n, k];
      int row = 0;
      foreach (var r in rows) {
        int col = 0;
        foreach (var info in parameterEntries) {
          if (ds.VariableHasType<double>(info.variableName)) {
            x[row, col] = ds.GetDoubleValue(info.variableName, r + info.lag);
          } else if (ds.VariableHasType<string>(info.variableName)) {
            x[row, col] = ds.GetStringValue(info.variableName, r) == info.variableValue ? 1 : 0;
          } else throw new InvalidProgramException("found a variable of unknown type");
          col++;
        }
        row++;
      }
      double[] y = ds.GetDoubleValues(problemData.TargetVariable, rows).ToArray();


      alglib.ndimensional_rep xrep = (p, f, obj) => iterationCallback(p, f, obj);

      try {
        alglib.minlmcreatevj(y.Length, c, out var lmstate);
        alglib.minlmsetcond(lmstate, 0.0, maxIterations);
        alglib.minlmsetxrep(lmstate, iterationCallback != null);
        // alglib.minlmoptguardgradient(lmstate, 1e-5); // for debugging gradient calculation
        alglib.minlmoptimize(lmstate, CreateFunc(func, x, y), CreateJac(func_grad, x, y), xrep, rowEvaluationsCounter);
        alglib.minlmresults(lmstate, out c, out rep);
        // alglib.minlmoptguardresults(lmstate, out var optGuardReport);
      } catch (ArithmeticException) {
        return originalQuality;
      } catch (alglib.alglibexception) {
        return originalQuality;
      }

      counter.FunctionEvaluations += rowEvaluationsCounter.FunctionEvaluations / n;
      counter.GradientEvaluations += rowEvaluationsCounter.GradientEvaluations / n;

      // * TerminationType, completion code:
      //     * -8    optimizer detected NAN/INF values either in the function itself,
      //             or in its Jacobian
      //     * -5    inappropriate solver was used:
      //             * solver created with minlmcreatefgh() used  on  problem  with
      //               general linear constraints (set with minlmsetlc() call).
      //     * -3    constraints are inconsistent
      //     *  2    relative step is no more than EpsX.
      //     *  5    MaxIts steps was taken
      //     *  7    stopping conditions are too stringent,
      //             further improvement is impossible
      //     *  8    terminated   by  user  who  called  MinLMRequestTermination().
      //             X contains point which was "current accepted" when termination
      //             request was submitted.
      if (rep.terminationtype > 0) {
        UpdateParameters(tree, c, updateVariableWeights);
      }
      var quality = SymbolicRegressionSingleObjectiveMeanSquaredErrorEvaluator.Calculate(
        tree, problemData, rows,
        interpreter, applyLinearScaling: false,
        lowerEstimationLimit, upperEstimationLimit);

      if (!updateParametersInTree) UpdateParameters(tree, initialParameters, updateVariableWeights);

      if (originalQuality < quality || double.IsNaN(quality)) {
        UpdateParameters(tree, initialParameters, updateVariableWeights);
        return originalQuality;
      }
      return quality;
    }

    private static void UpdateParameters(ISymbolicExpressionTree tree, double[] parameters, bool updateVariableWeights) {
      int i = 0;
      foreach (var node in tree.Root.IterateNodesPrefix().OfType<SymbolicExpressionTreeTerminalNode>()) {
        NumberTreeNode numberTreeNode = node as NumberTreeNode;
        VariableTreeNodeBase variableTreeNodeBase = node as VariableTreeNodeBase;
        FactorVariableTreeNode factorVarTreeNode = node as FactorVariableTreeNode;
        if (numberTreeNode != null) {
          if (numberTreeNode.Parent.Symbol is Power
              && numberTreeNode.Parent.GetSubtree(1) == numberTreeNode) continue; // exponents in powers are not optimized (see TreeToAutoDiffTermConverter)
          numberTreeNode.Value = parameters[i++];
        } else if (updateVariableWeights && variableTreeNodeBase != null)
          variableTreeNodeBase.Weight = parameters[i++];
        else if (factorVarTreeNode != null) {
          for (int j = 0; j < factorVarTreeNode.Weights.Length; j++)
            factorVarTreeNode.Weights[j] = parameters[i++];
        }
      }
    }

    private static alglib.ndimensional_fvec CreateFunc(TreeToAutoDiffTermConverter.ParametricFunction func, double[,] x, double[] y) {
      int d = x.GetLength(1);
      // row buffer
      var xi = new double[d];
      // function must return residuals, alglib optimizes resid²
      return (double[] c, double[] resid, object o) => {
        for (int i = 0; i < y.Length; i++) {
          Buffer.BlockCopy(x, i * d * sizeof(double), xi, 0, d * sizeof(double)); // copy row. We are using BlockCopy instead of Array.Copy because x has rank 2
          resid[i] = func(c, xi) - y[i];
        }
        var counter = (EvaluationsCounter)o;
        counter.FunctionEvaluations += y.Length;
      };
    }

    private static alglib.ndimensional_jac CreateJac(TreeToAutoDiffTermConverter.ParametricFunctionGradient func_grad, double[,] x, double[] y) {
      int numParams = x.GetLength(1);
      // row buffer
      var xi = new double[numParams];
      return (double[] c, double[] resid, double[,] jac, object o) => {
        int numVars = c.Length;
        for (int i = 0; i < y.Length; i++) {
          Buffer.BlockCopy(x, i * numParams * sizeof(double), xi, 0, numParams * sizeof(double)); // copy row
          var tuple = func_grad(c, xi);
          resid[i] = tuple.Item2 - y[i];
          Buffer.BlockCopy(tuple.Item1, 0, jac, i * numVars * sizeof(double), numVars * sizeof(double)); // copy the gradient to jac. BlockCopy because jac has rank 2.
        }
        var counter = (EvaluationsCounter)o;
        counter.FunctionEvaluations += y.Length;
        counter.GradientEvaluations += y.Length;
      };
    }

    public class EvaluationsCounter {
      public int FunctionEvaluations = 0;
      public int GradientEvaluations = 0;
    }

    #endregion
  }
}
