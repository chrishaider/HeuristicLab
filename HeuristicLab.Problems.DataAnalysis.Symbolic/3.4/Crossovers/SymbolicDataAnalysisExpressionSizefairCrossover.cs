﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2013 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Common;
using HeuristicLab.Core;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Crossovers {
  [Item("SizeFairCrossover", "An operator which performs subtree swapping crossover.")]
  class SymbolicDataAnalysisExpressionSizefairCrossover<T> : SubtreeCrossover, ISymbolicDataAnalysisExpressionCrossover<T> {
    [StorableConstructor]
    protected SymbolicDataAnalysisExpressionSizefairCrossover(bool deserializing) : base(deserializing) { }
    protected SymbolicDataAnalysisExpressionSizefairCrossover(SubtreeCrossover original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) { return new SymbolicDataAnalysisExpressionSizefairCrossover<T>(this, cloner); }

    public override bool CanChangeName {
      get { return true; }
    }

    public SymbolicDataAnalysisExpressionSizefairCrossover()
      : base() {
      SymbolicDataAnalysisEvaluationPartitionParameter.Hidden = true;
      Name = "SizeFairCrossover";
    }
  }
}