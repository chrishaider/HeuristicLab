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

using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.DataAnalysis {
  /// <summary>
  /// Represents discriminant function classification data analysis models.
  /// </summary>
  [StorableClass]
  [Item("DiscriminantFunctionClassificationModel", "Represents a classification model that uses a discriminant function and classification thresholds.")]
  public class DiscriminantFunctionClassificationModel : NamedItem, IDiscriminantFunctionClassificationModel {
    [Storable]
    private IRegressionModel model;

    [Storable]
    private double[] classValues;
    public IEnumerable<double> ClassValues {
      get { return (double[])classValues.Clone(); }
      private set { classValues = value.ToArray(); }
    }

    [Storable]
    private double[] thresholds;
    public IEnumerable<double> Thresholds {
      get { return (IEnumerable<double>)thresholds.Clone(); }
      private set { thresholds = value.ToArray(); }
    }


    [StorableConstructor]
    protected DiscriminantFunctionClassificationModel(bool deserializing) : base(deserializing) { }
    protected DiscriminantFunctionClassificationModel(DiscriminantFunctionClassificationModel original, Cloner cloner)
      : base(original, cloner) {
      model = cloner.Clone(original.model);
      classValues = (double[])original.classValues.Clone();
      thresholds = (double[])original.thresholds.Clone();
    }

    public DiscriminantFunctionClassificationModel(IRegressionModel model)
      : base() {
      this.name = ItemName;
      this.description = ItemDescription;
      this.model = model;
      this.classValues = new double[] { 0.0 };
      this.thresholds = new double[] { double.NegativeInfinity };
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new DiscriminantFunctionClassificationModel(this, cloner);
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
      return model.GetEstimatedValues(dataset, rows);
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
