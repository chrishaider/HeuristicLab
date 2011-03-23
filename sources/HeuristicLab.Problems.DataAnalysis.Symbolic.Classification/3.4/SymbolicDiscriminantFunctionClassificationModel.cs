#region License Information
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

using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Optimization;
using System;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Classification {
  /// <summary>
  /// Represents a symbolic classification model
  /// </summary>
  [StorableClass]
  [Item(Name = "SymbolicDiscriminantFunctionClassificationModel", Description = "Represents a symbolic classification model unsing a discriminant function.")]
  public class SymbolicDiscriminantFunctionClassificationModel : SymbolicDataAnalysisModel, ISymbolicDiscriminantFunctionClassificationModel {

    [Storable]
    private double[] thresholds;
    public IEnumerable<double> Thresholds {
      get { return (IEnumerable<double>)thresholds.Clone(); }
      private set { thresholds = value.ToArray(); }
    }
    [Storable]
    private double[] classValues;
    public IEnumerable<double> ClassValues {
      get { return (IEnumerable<double>)classValues.Clone(); }
      private set { classValues = value.ToArray(); }
    }
    [Storable]
    private double lowerEstimationLimit;
    public double LowerEstimationLimit { get { return lowerEstimationLimit; } }
    [Storable]
    private double upperEstimationLimit;
    public double UpperEstimationLimit { get { return upperEstimationLimit; } }

    [StorableConstructor]
    protected SymbolicDiscriminantFunctionClassificationModel(bool deserializing) : base(deserializing) { }
    protected SymbolicDiscriminantFunctionClassificationModel(SymbolicDiscriminantFunctionClassificationModel original, Cloner cloner)
      : base(original, cloner) {
      classValues = (double[])original.classValues.Clone();
      thresholds = (double[])original.thresholds.Clone();
      lowerEstimationLimit = original.lowerEstimationLimit;
      upperEstimationLimit = original.upperEstimationLimit;
    }
    public SymbolicDiscriminantFunctionClassificationModel(ISymbolicExpressionTree tree, ISymbolicDataAnalysisExpressionTreeInterpreter interpreter,
      double lowerEstimationLimit = double.MinValue, double upperEstimationLimit = double.MaxValue)
      : base(tree, interpreter) {
      thresholds = new double[] { double.NegativeInfinity };
      classValues = new double[] { 0.0 };
      this.lowerEstimationLimit = lowerEstimationLimit;
      this.upperEstimationLimit = upperEstimationLimit;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new SymbolicDiscriminantFunctionClassificationModel(this, cloner);
    }

    public void SetThresholdsAndClassValues(IEnumerable<double> thresholds, IEnumerable<double> classValues) {
      var classValuesArr = classValues.ToArray();
      var thresholdsArr = thresholds.ToArray();
      if (thresholdsArr.Length != classValuesArr.Length) throw new ArgumentException();

      this.classValues = classValuesArr;
      this.thresholds = thresholdsArr;
      OnThresholdsChanged(EventArgs.Empty);
    }

    public IEnumerable<double> GetEstimatedValues(Dataset dataset, IEnumerable<int> rows) {
      return Interpreter.GetSymbolicExpressionTreeValues(SymbolicExpressionTree, dataset, rows)
        .LimitToRange(lowerEstimationLimit, upperEstimationLimit);
    }

    public IEnumerable<double> GetEstimatedClassValues(Dataset dataset, IEnumerable<int> rows) {
      foreach (var x in GetEstimatedValues(dataset, rows)) {
        int classIndex = 0;
        // find first threshold value which is larger than x => class index = threshold index + 1
        for (int i = 0; i < thresholds.Length; i++) {
          if (x > thresholds[i]) classIndex++;
          else break;
        }
        yield return classValues.ElementAt(classIndex - 1);
      }
    }

    #region events
    public event EventHandler ThresholdsChanged;
    protected virtual void OnThresholdsChanged(EventArgs e) {
      var listener = ThresholdsChanged;
      if (listener != null) listener(this, e);
    }
    #endregion
  }
}
