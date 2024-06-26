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

using HeuristicLab.Common;
using HeuristicLab.Core;
using HEAL.Attic;

namespace HeuristicLab.Optimization.Operators {
  /// <summary>
  /// Modifies the value by linear (constant) fall or rise.
  /// </summary>
  [Item("LinearDiscreteDoubleValueModifier", "Modifies the value by linear (constant) fall or rise.")]
  [StorableType("B0EFCF88-9463-43BA-A0E6-0E0ACB0E4793")]
  public class LinearDiscreteDoubleValueModifier : DiscreteDoubleValueModifier {
    [StorableConstructor]
    protected LinearDiscreteDoubleValueModifier(StorableConstructorFlag _) : base(_) { }
    protected LinearDiscreteDoubleValueModifier(LinearDiscreteDoubleValueModifier original, Cloner cloner) : base(original, cloner) { }
    public LinearDiscreteDoubleValueModifier() : base() { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new LinearDiscreteDoubleValueModifier(this, cloner);
    }

    protected override double Modify(double value, double startValue, double endValue, int index, int startIndex, int endIndex) {
      return Apply(value, startValue, endValue, index, startIndex, endIndex);
    }

    /// <summary>
    /// Calculates a new value based on linear decay or growth.
    /// </summary>
    /// <param name="value">The previous value.</param>
    /// <param name="startValue">The initial value.</param>
    /// <param name="endValue">The final value.</param>
    /// <param name="index">The current index.</param>
    /// <param name="startIndex">The initial index.</param>
    /// <param name="endIndex">The final index.</param>
    /// <returns>The new value.</returns>
    public static double Apply(double value, double startValue, double endValue, int index, int startIndex, int endIndex) {
      double k = (endValue - startValue) / (endIndex - startIndex);
      double x = index - startIndex;
      return k * x + startValue;
    }
  }
}
