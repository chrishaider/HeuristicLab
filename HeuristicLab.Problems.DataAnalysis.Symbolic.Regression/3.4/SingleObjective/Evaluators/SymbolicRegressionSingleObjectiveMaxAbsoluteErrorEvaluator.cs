﻿#region License Information
/* HeuristicLab
 * Copyright (C) Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

using System.Collections.Generic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [Item("Maximum absolute error Evaluator", "Calculates the maximum squared error of a symbolic regression solution.")]
  [StorableType("256A6405-D1EE-4D8D-963A-42C56FEE8571")]
  public class SymbolicRegressionSingleObjectiveMaxAbsoluteErrorEvaluator : SymbolicRegressionSingleObjectiveEvaluator {
    public override bool Maximization { get { return false; } }
    [StorableConstructor]
    protected SymbolicRegressionSingleObjectiveMaxAbsoluteErrorEvaluator(StorableConstructorFlag _) : base(_) { }
    protected SymbolicRegressionSingleObjectiveMaxAbsoluteErrorEvaluator(SymbolicRegressionSingleObjectiveMaxAbsoluteErrorEvaluator original, Cloner cloner)
      : base(original, cloner) {
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicRegressionSingleObjectiveMaxAbsoluteErrorEvaluator(this, cloner);
    }
    public SymbolicRegressionSingleObjectiveMaxAbsoluteErrorEvaluator() : base() { }

    public override IOperation InstrumentedApply() {
      var tree = SymbolicExpressionTreeParameter.ActualValue;
      IEnumerable<int> rows = GenerateRowsToEvaluate();

      double quality = Calculate(
        tree, ProblemDataParameter.ActualValue,
        rows, SymbolicDataAnalysisTreeInterpreterParameter.ActualValue,
        ApplyLinearScalingParameter.ActualValue.Value,
        EstimationLimitsParameter.ActualValue.Lower, 
        EstimationLimitsParameter.ActualValue.Upper);
      QualityParameter.ActualValue = new DoubleValue(quality);

      return base.InstrumentedApply();
    }

    public static double Calculate(
      ISymbolicExpressionTree tree,
      IRegressionProblemData problemData,
      IEnumerable<int> rows,
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      bool applyLinearScaling,
      double lowerEstimationLimit, 
      double upperEstimationLimit) {
      IEnumerable<double> estimatedValues = interpreter.GetSymbolicExpressionTreeValues(tree, problemData.Dataset, rows);
      IEnumerable<double> targetValues = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, rows);
      OnlineCalculatorError errorState;

      double mse;
      if (applyLinearScaling) {
        var maeCalculator = new OnlineMaxAbsoluteErrorCalculator();
        CalculateWithScaling(targetValues, estimatedValues, lowerEstimationLimit, upperEstimationLimit, maeCalculator, problemData.Dataset.Rows);
        errorState = maeCalculator.ErrorState;
        mse = maeCalculator.MaxAbsoluteError;
      } else {
        IEnumerable<double> boundedEstimatedValues = estimatedValues.LimitToRange(lowerEstimationLimit, upperEstimationLimit);
        mse = OnlineMaxAbsoluteErrorCalculator.Calculate(targetValues, boundedEstimatedValues, out errorState);
      }
      if (errorState != OnlineCalculatorError.None) return double.NaN;
      return mse;
    }

    public override double Evaluate(IExecutionContext context, ISymbolicExpressionTree tree, IRegressionProblemData problemData, IEnumerable<int> rows) {
      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = context;
      EstimationLimitsParameter.ExecutionContext = context;
      ApplyLinearScalingParameter.ExecutionContext = context;

      double mse = Calculate(
        tree, problemData, rows,
        SymbolicDataAnalysisTreeInterpreterParameter.ActualValue, 
        ApplyLinearScalingParameter.ActualValue.Value,
        EstimationLimitsParameter.ActualValue.Lower, 
        EstimationLimitsParameter.ActualValue.Upper);

      SymbolicDataAnalysisTreeInterpreterParameter.ExecutionContext = null;
      EstimationLimitsParameter.ExecutionContext = null;
      ApplyLinearScalingParameter.ExecutionContext = null;

      return mse;
    }

    public override double Evaluate(
      ISymbolicExpressionTree tree,
      IRegressionProblemData problemData,
      IEnumerable<int> rows,
      ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      bool applyLinearScaling = true,
      double lowerEstimationLimit = double.MinValue,
      double upperEstimationLimit = double.MaxValue) {
      return Calculate(tree, problemData, rows, interpreter, applyLinearScaling, lowerEstimationLimit, upperEstimationLimit);
    }
  }
}