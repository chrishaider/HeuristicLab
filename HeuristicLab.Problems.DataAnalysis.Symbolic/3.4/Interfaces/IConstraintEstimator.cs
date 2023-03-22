using HEAL.Attic;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("14D51403-B616-4779-A99D-3B6C19CF5748")]
  public interface IConstraintEstimator : INamedItem, IStatefulItem {
    bool IsCompatible(ISymbolicExpressionTree tree);
    int EvaluatedSolutions { get; set; }
  }
}