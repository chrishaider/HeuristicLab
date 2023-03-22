using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Core;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("D3C39C30-7E45-4951-8697-3AC0C6BE1757")]
  public interface IOptimisticEstimator : IConstraintEstimator {
    double GetConstraintViolation(ISymbolicExpressionTree tree, IRegressionProblemData problemData, ShapeConstraint constraint, IEnumerable<int> rows);
  }
}
