﻿#region License Information

/* HeuristicLab
 * Copyright (C) 2002-2015 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Data;
using HeuristicLab.Encodings.BinaryVectorEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Problems.Binary {
  [StorableClass]
  public abstract class BinaryProblem : SingleObjectiveBasicProblem<BinaryVectorEncoding> {

    public virtual int Length {
      get { return Encoding.Length; }
      set { Encoding.Length = value; }
    }

    [StorableConstructor]
    protected BinaryProblem(bool deserializing) : base(deserializing) { }
    protected BinaryProblem(BinaryProblem original, Cloner cloner) : base(original, cloner) { }
    protected BinaryProblem() : base() { }

    public virtual bool IsBetter(double quality, double bestQuality) {
      return (Maximization && quality > bestQuality || !Maximization && quality < bestQuality);
    }

    public sealed override double Evaluate(Individual individual, IRandom random) {
      return Evaluate(individual.BinaryVector(), random);
    }

    public abstract double Evaluate(BinaryVector vector, IRandom random);
  }
}