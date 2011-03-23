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

namespace HeuristicLab.Problems.DataAnalysis {
  public class OnlineLinearScalingParameterCalculator {

    /// <summary>
    /// Additive constant
    /// </summary>
    public double Alpha {
      get {
        if (cnt < 2)
          return 0;
        else
          return targetMeanCalculator.Mean - Beta * originalMeanAndVarianceCalculator.Mean;
      }
    }

    /// <summary>
    /// Multiplicative factor
    /// </summary>
    public double Beta {
      get {
        if (cnt < 2)
          return 1;
        else if (originalMeanAndVarianceCalculator.PopulationVariance.IsAlmost(0.0))
          return 1;
        else
          return originalTargetCovarianceEvaluator.Covariance / originalMeanAndVarianceCalculator.PopulationVariance;
      }
    }

    private int cnt;
    private OnlineMeanAndVarianceCalculator targetMeanCalculator;
    private OnlineMeanAndVarianceCalculator originalMeanAndVarianceCalculator;
    private OnlineCovarianceEvaluator originalTargetCovarianceEvaluator;

    public OnlineLinearScalingParameterCalculator() {
      targetMeanCalculator = new OnlineMeanAndVarianceCalculator();
      originalMeanAndVarianceCalculator = new OnlineMeanAndVarianceCalculator();
      originalTargetCovarianceEvaluator = new OnlineCovarianceEvaluator();
      Reset();
    }

    public void Reset() {
      cnt = 0;
      targetMeanCalculator.Reset();
      originalMeanAndVarianceCalculator.Reset();
      originalTargetCovarianceEvaluator.Reset();
    }

    /// <summary>
    /// Calculates linear scaling parameters in one pass.
    /// The formulas to calculate the scaling parameters were taken from Scaled Symblic Regression by Maarten Keijzer.
    /// http://www.springerlink.com/content/x035121165125175/
    /// </summary>
    public void Add(double original, double target) {
      // validity of values is checked in mean calculator and covariance calculator
      targetMeanCalculator.Add(target);
      originalMeanAndVarianceCalculator.Add(original);
      originalTargetCovarianceEvaluator.Add(original, target);

      cnt++;
    }

    /// <summary>
    /// Calculates alpha and beta parameters to linearly scale elements of original to the scale and location of target
    /// original[i] * beta + alpha
    /// </summary>
    /// <param name="original">Values that should be scaled</param>
    /// <param name="target">Target values to which the original values should be scaled</param>
    /// <param name="alpha">Additive constant for the linear scaling</param>
    /// <param name="beta">Multiplicative factor for the linear scaling</param>
    public static void Calculate(IEnumerable<double> original, IEnumerable<double> target, out double alpha, out double beta) {
      OnlineLinearScalingParameterCalculator calculator = new OnlineLinearScalingParameterCalculator();
      IEnumerator<double> originalEnumerator = original.GetEnumerator();
      IEnumerator<double> targetEnumerator = target.GetEnumerator();

      // always move forward both enumerators (do not use short-circuit evaluation!)
      while (originalEnumerator.MoveNext() & targetEnumerator.MoveNext()) {
        double originalElement = originalEnumerator.Current;
        double targetElement = targetEnumerator.Current;
        // don't consider very large or very small values for scaling
        // careful: this also excludes infinity and NaN values
        if (originalElement > -1.0E07 && originalElement < 1.0E07) {
          calculator.Add(originalElement, targetElement);
        }
      }

      // check if both enumerators are at the end to make sure both enumerations have the same length
      if (originalEnumerator.MoveNext() || targetEnumerator.MoveNext()) {
        throw new ArgumentException("Number of elements in original and target enumeration do not match.");
      } else {
        alpha = calculator.Alpha;
        beta = calculator.Beta;
      }
    }
  }
}
