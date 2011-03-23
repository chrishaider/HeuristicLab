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

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [Item("Pearson R² & Tree size Evaluator", "Calculates the Pearson R² and the tree size of a symbolic regression solution.")]
  [StorableClass]
  public class SymbolicRegressionMultiObjectivePearsonRSquaredTreeSizeEvaluator : SymbolicRegressionMultiObjectiveEvaluator {
    [StorableConstructor]
    protected SymbolicRegressionMultiObjectivePearsonRSquaredTreeSizeEvaluator(bool deserializing) : base(deserializing) { }
    protected SymbolicRegressionMultiObjectivePearsonRSquaredTreeSizeEvaluator(SymbolicRegressionMultiObjectivePearsonRSquaredTreeSizeEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicRegressionMultiObjectivePearsonRSquaredTreeSizeEvaluator(this, cloner);
    }

    public SymbolicRegressionMultiObjectivePearsonRSquaredTreeSizeEvaluator() : base() { }

    public override IEnumerable<bool> Maximization { get { return new bool[2] { true, false }; } }

    public override IOperation Apply() {
      IEnumerable<int> rows = GenerateRowsToEvaluate();
      double[] qualities = Calculate(SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, SymbolicExpressionTreeParameter.ActualValue, EstimationLimitsParameter.ActualValue.Lower, EstimationLimitsParameter.ActualValue.Upper, ProblemDataParameter.ActualValue, rows);
      QualitiesParameter.ActualValue = new DoubleArray(qualities);
      return base.Apply();
    }

    public static double[] Calculate(ISymbolicDataAnalysisExpressionTreeInterpreter interpreter, ISymbolicExpressionTree solution, double lowerEstimationLimit, double upperEstimationLimit, IRegressionProblemData problemData, IEnumerable<int> rows) {
      IEnumerable<double> estimatedValues = interpreter.GetSymbolicExpressionTreeValues(solution, problemData.Dataset, rows);
      IEnumerable<double> originalValues = problemData.Dataset.GetEnumeratedVariableValues(problemData.TargetVariable, rows);
      try {
        double r2 = OnlinePearsonsRSquaredEvaluator.Calculate(originalValues, estimatedValues);
        return new double[2] { r2, solution.Length };
      }
      catch (ArgumentException) {
        // if R² cannot be calcualted return worst possible fitness value
        return new double[2] { 0.0, solution.Length };
      }
    }

    public override double[] Evaluate(IExecutionContext context, ISymbolicExpressionTree tree, IRegressionProblemData problemData, IEnumerable<int> rows) {
      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = context;
      EstimationLimitsParameter.ExecutionContext = context;

      double[] quality = Calculate(SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, tree, EstimationLimitsParameter.ActualValue.Lower, EstimationLimitsParameter.ActualValue.Upper, problemData, rows);

      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = null;
      EstimationLimitsParameter.ExecutionContext = null;

      return quality;
    }
  }
}
