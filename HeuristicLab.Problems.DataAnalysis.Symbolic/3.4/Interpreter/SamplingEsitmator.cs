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
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.DataAnalysis.Symbolic;
using Variable = HeuristicLab.Problems.DataAnalysis.Symbolic.Variable;

namespace HeuristicLab.Problems.DataAnalysis.Symbolicr {
  [StorableType("0FDA3604-9526-44E5-819A-C2FC275619A8")]
  [Item("Sampling Estimator", "Estimates the bounds of the model with samples.")]
  public sealed class SamplingEsitmator : ParameterizedNamedItem, IPessimisticEstimator {
    #region Parameters

    private const string EvaluatedSolutionsParameterName = "EvaluatedSolutions";
    private const string SamplingSizeParameterName = "SamplingSize";

    public IFixedValueParameter<IntValue> EvaluatedSolutionsParameter =>
      (IFixedValueParameter<IntValue>)Parameters[EvaluatedSolutionsParameterName];


    public IFixedValueParameter<IntValue> SamplingSizeParameter=>
      (IFixedValueParameter<IntValue>)Parameters[SamplingSizeParameterName];


    public int EvaluatedSolutions {
      get => EvaluatedSolutionsParameter.Value.Value;
      set => EvaluatedSolutionsParameter.Value.Value = value;
    }

    public int SamplingSize {
      get => SamplingSizeParameter.Value.Value;
      set => SamplingSizeParameter.Value.Value = value;
    }

    public Dataset Samples { get; set; }
    #endregion

    #region Constructors

    [StorableConstructor]
    private SamplingEsitmator(StorableConstructorFlag _) : base(_) { }

    private SamplingEsitmator(SamplingEsitmator original, Cloner cloner) : base(original, cloner) { }

    public SamplingEsitmator() : base("Sampling Estimator",
      "Estimates the bounds of the model with samples") {
      Parameters.Add(new FixedValueParameter<IntValue>(EvaluatedSolutionsParameterName, "A counter for the total number of solutions the estimator has evaluated", new IntValue(0)));
      Parameters.Add(new FixedValueParameter<IntValue>(SamplingSizeParameterName, "A parameter to set the amount of samples to be taken.", new IntValue(1000)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SamplingEsitmator(this, cloner);
    }

    #endregion

    public void ClearState() {
      EvaluatedSolutions = 0;
    }
    public double GetConstraintViolation(ISymbolicExpressionTree tree, IntervalCollection variableRanges, ShapeConstraint constraint) {

      var rows = Samples.Rows;


      for (var i = 0; i < rows; ++i) {
        
      }

      Console.WriteLine(Samples);

      return 0;
    }

    public Interval GetModelBound(ISymbolicExpressionTree tree, IntervalCollection variableRanges) {
      throw new NotImplementedException();
    }

    public IDictionary<ISymbolicExpressionTreeNode, Interval> GetModelNodeBounds(ISymbolicExpressionTree tree, IntervalCollection variableRanges) {
      throw new NotImplementedException();
    }

    public void InitializeState() {
      EvaluatedSolutions = 0;
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
