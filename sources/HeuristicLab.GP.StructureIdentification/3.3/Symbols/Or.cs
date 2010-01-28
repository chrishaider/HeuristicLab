#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2008 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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


namespace HeuristicLab.GP.StructureIdentification {
  public sealed class Or : Function {
    public override string Description {
      get {
        return @"Logical OR operation. Only defined for sub-tree-results 0.0 and 1.0.";
      }
    }

    public Or()
      : base() {
      MinSubTrees = 2; MaxSubTrees = 3;
    }
    public override HeuristicLab.GP.Interfaces.IFunctionTree GetTreeNode() {
      return new FunctionTreeBase(this);
    }
  }
}
