#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2010 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

using HeuristicLab.Core;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Permutation {
  /// <summary>
  /// A base class for permutation manipulation operators.
  /// </summary>
  [Item("PermutationManipulator", "A base class for permutation manipulation operators.")]
  [EmptyStorableClass]
  public abstract class PermutationManipulator : SingleSuccessorOperator, IPermutationManipulationOperator {
    public ILookupParameter<IRandom> RandomParameter {
      get { return (LookupParameter<IRandom>)Parameters["Random"]; }
    }
    public LookupParameter<Permutation> PermutationParameter {
      get { return (LookupParameter<Permutation>)Parameters["Permutation"]; }
    }

    protected PermutationManipulator()
      : base() {
      Parameters.Add(new LookupParameter<IRandom>("Random", "The pseudo random number generator which should be used for stochastic manipulation operators."));
      Parameters.Add(new LookupParameter<Permutation>("Permutation", "The permutation which should be manipulated."));
    }

    public sealed override IOperation Apply() {
      Manipulate(RandomParameter.ActualValue, PermutationParameter.ActualValue);
      return base.Apply();
    }

    protected abstract void Manipulate(IRandom random, Permutation permutation);
  }
}
