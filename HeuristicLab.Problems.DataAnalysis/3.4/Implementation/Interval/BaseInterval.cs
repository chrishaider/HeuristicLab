#region License Information

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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("20E3DEE7-4008-4436-AC98-61C99C2E8254")]
  public abstract class BaseInterval : IInterval {
    #region Props
    [Storable]
    public double LowerBound { get; private set; }
    [Storable]
    public double UpperBound { get; private set; }
    public double Width => Math.Abs(LowerBound - UpperBound);
    public bool isProper => LowerBound <= UpperBound;
    #endregion

    #region Ctrs
    [StorableConstructor]
    protected BaseInterval(StorableConstructorFlag _) { }
    public BaseInterval(double lowerBound, double upperBound) {
      LowerBound = lowerBound;
      UpperBound = upperBound;
    }
    public BaseInterval(double v) : this(v, v) { }
    #endregion

    #region Equals/Hash
    public bool Equals(IInterval other) {
      if (other == null) return false;

      return (UpperBound == other.UpperBound || (double.IsNaN(UpperBound) && double.IsNaN(other.UpperBound)))
        && (LowerBound == other.LowerBound || (double.IsNaN(LowerBound) && double.IsNaN(other.LowerBound)));
    }

    public override bool Equals(object obj) {
      return base.Equals(obj as IInterval);
    }

    public override int GetHashCode() {
      return LowerBound.GetHashCode() ^ UpperBound.GetHashCode();
    }

    public static bool operator ==(BaseInterval left, BaseInterval right) {
      if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
      return left.Equals(right);
    }

    public static bool operator !=(BaseInterval left, BaseInterval right) {
      return !(left == right);
    }
    #endregion

    #region Methods
    public bool Contains(double value) {
      return LowerBound <= value && value <= UpperBound;
    }

    public bool Contains(IInterval other) {
      if (double.IsNegativeInfinity(LowerBound) && double.IsPositiveInfinity(UpperBound)) return true;
      if (other.LowerBound >= LowerBound && other.UpperBound <= UpperBound) return true;

      return false;
    }
    #endregion  
  }
}
