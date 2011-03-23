﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2011 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Classification {
  [Item("Pearson R² evaluator", "Calculates the square of the pearson correlation coefficient (also known as coefficient of determination) of a symbolic classification solution.")]
  [StorableClass]
  public class SymbolicClassificationSingleObjectivePearsonRSquaredEvaluator : SymbolicClassificationSingleObjectiveEvaluator {
    [StorableConstructor]
    protected SymbolicClassificationSingleObjectivePearsonRSquaredEvaluator(bool deserializing) : base(deserializing) { }
    protected SymbolicClassificationSingleObjectivePearsonRSquaredEvaluator(SymbolicClassificationSingleObjectivePearsonRSquaredEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicClassificationSingleObjectivePearsonRSquaredEvaluator(this, cloner);
    }

    public SymbolicClassificationSingleObjectivePearsonRSquaredEvaluator() : base() { }

    public override bool Maximization { get { return true; } }

    public override IOperation Apply() {
      IEnumerable<int> rows = GenerateRowsToEvaluate();
      double quality = Calculate(SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, SymbolicExpressionTreeParameter.ActualValue, EstimationLimitsParameter.ActualValue.Lower, EstimationLimitsParameter.ActualValue.Upper, ProblemDataParameter.ActualValue, rows);
      QualityParameter.ActualValue = new DoubleValue(quality);
      return base.Apply();
    }

    public static double Calculate(ISymbolicDataAnalysisExpressionTreeInterpreter interpreter, ISymbolicExpressionTree solution, double lowerEstimationLimit, double upperEstimationLimit, IClassificationProblemData problemData, IEnumerable<int> rows) {
      IEnumerable<double> estimatedValues = interpreter.GetSymbolicExpressionTreeValues(solution, problemData.Dataset, rows);
      IEnumerable<double> originalValues = problemData.Dataset.GetEnumeratedVariableValues(problemData.TargetVariable, rows);
      try {
        return OnlinePearsonsRSquaredEvaluator.Calculate(originalValues, estimatedValues);
      }
      catch (ArgumentException) {
        // if R² cannot be calculated because of NaN or ininity elements => return worst possible fitness valuse
        return 0.0;
      }
    }

    public override double Evaluate(IExecutionContext context, ISymbolicExpressionTree tree, IClassificationProblemData problemData, IEnumerable<int> rows) {
      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = context;
      EstimationLimitsParameter.ExecutionContext = context;

      double r2 = Calculate(SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, tree, EstimationLimitsParameter.ActualValue.Lower, EstimationLimitsParameter.ActualValue.Upper, problemData, rows);

      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = null;
      EstimationLimitsParameter.ExecutionContext = null;

      return r2;
    }
  }
}
