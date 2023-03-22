using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("487A5696-8D1A-44BA-997E-1D8D67B75C92")]
  [Item("Sampling Optimistic Estimator", "Sampling Estimator for shape constrained regression models.")]
  public sealed class SamplingOptimisticEstimator : ParameterizedNamedItem, IOptimisticEstimator{

    #region Parameters
    private const string EvaluatedSolutionsParameterName = "EvaluatedSolutions";

    public IFixedValueParameter<IntValue> EvaluatedSolutionsParameter =>
      (IFixedValueParameter<IntValue>)Parameters[EvaluatedSolutionsParameterName];

    public int EvaluatedSolutions {
      get => EvaluatedSolutionsParameter.Value.Value;
      set => EvaluatedSolutionsParameter.Value.Value = value;
    }
    #endregion

    #region Constructors
    [StorableConstructor]
    private SamplingOptimisticEstimator(StorableConstructorFlag _) : base(_) { }
    private SamplingOptimisticEstimator(SamplingOptimisticEstimator original, Cloner cloner) : base(original, cloner) { }
    public SamplingOptimisticEstimator() : base("Sampling Optimistic Estimator",
      "Sampling Estimator for shape constrained regression models.") {
      Parameters.Add(new FixedValueParameter<IntValue>(EvaluatedSolutionsParameterName,
        "A counter for the total number of solutions the estimator has evaluated.", new IntValue(0)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SamplingOptimisticEstimator(this, cloner);
    }
    #endregion

    #region IStatefulItem Member
    private readonly object syncRoot = new object();

    public void InitializeState() {
      EvaluatedSolutions = 0;
    }

    public void ClearState() { }
    #endregion


    public double GetConstraintViolation(ISymbolicExpressionTree tree, IRegressionProblemData problemData, ShapeConstraint constraint, IEnumerable<int> rows) {
      var interpreter = new SymbolicDataAnalysisExpressionTreeLinearInterpreter();
      var modelImages = interpreter.GetSymbolicExpressionTreeValues(tree, problemData.Dataset, rows);

      var violation = new List<double>();


      //Check for constraint regions
      if(constraint.Regions.Count == 0) {
        foreach(var image in modelImages) {
          if(constraint.Interval.Contains(image)) continue;
          violation.Add(image < constraint.Interval.LowerBound
            ? Math.Abs(image - constraint.Interval.LowerBound)
            : Math.Abs(image - constraint.Interval.UpperBound));
        }
      } else {
        //only one region can be specified per constraint
        var region = constraint.Regions.GetDictionary().First();

        for(var i = rows.First(); i < rows.Last(); ++i) {
          if(!region.Value.Contains(problemData.Dataset.GetDoubleValue(region.Key, i))) continue;

          violation.Add(modelImages.ElementAt(i) < constraint.Interval.LowerBound
            ? Math.Abs(modelImages.ElementAt(i) - constraint.Interval.LowerBound)
            : Math.Abs(modelImages.ElementAt(i) - constraint.Interval.UpperBound));
        }
      }

      return violation.Count > 0 ? violation.Max() : 0;
    }


    public bool IsCompatible(ISymbolicExpressionTree tree) {
      var containsUnknownSymbols = (
        from n in tree.Root.GetSubtree(0).IterateNodesPrefix()
        where
          !(n.Symbol is Variable) &&
          !(n.Symbol is Number) &&
          !(n.Symbol is Constant) &&
          !(n.Symbol is StartSymbol) &&
          !(n.Symbol is Addition) &&
          !(n.Symbol is Subtraction) &&
          !(n.Symbol is Multiplication) &&
          !(n.Symbol is Division) &&
          !(n.Symbol is Sine) &&
          !(n.Symbol is Cosine) &&
          !(n.Symbol is Tangent) &&
          !(n.Symbol is HyperbolicTangent) &&
          !(n.Symbol is Logarithm) &&
          !(n.Symbol is Exponential) &&
          !(n.Symbol is Square) &&
          !(n.Symbol is SquareRoot) &&
          !(n.Symbol is Cube) &&
          !(n.Symbol is CubeRoot) &&
          !(n.Symbol is Power) &&
          !(n.Symbol is Absolute) &&
          !(n.Symbol is AnalyticQuotient) &&
          !(n.Symbol is SubFunctionSymbol)
        select n).Any();
      return !containsUnknownSymbols;
    }
  }
}