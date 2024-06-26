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
using HEAL.Attic;
using HeuristicLab.Common;

namespace HeuristicLab.Encodings.SymbolicExpressionTreeEncoding {
  [StorableType("70D190B2-22F4-41E5-9938-EFD1B14ECF43")]
  public sealed class SimpleSymbolicExpressionGrammar : SymbolicExpressionGrammar {
    [StorableConstructor]
    private SimpleSymbolicExpressionGrammar(StorableConstructorFlag _) : base(_) { }
    private SimpleSymbolicExpressionGrammar(SimpleSymbolicExpressionGrammar original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) {
      return new SimpleSymbolicExpressionGrammar(this, cloner);
    }

    public SimpleSymbolicExpressionGrammar()
      : base("Simple Grammar", "A simple grammar containing symbols that differ only in their name.") {
    }

    public void AddSymbol(string symbolName, int minimumArity, int maximumArity) {
      AddSymbol(symbolName, string.Empty, minimumArity, maximumArity);
    }
    public void AddSymbol(string symbolName, string description, int minimumArity, int maximumArity) {
      var symbol = new SimpleSymbol(symbolName, description, minimumArity, maximumArity);
      AddSymbol(symbol);
      SetSubtreeCount(symbol, symbol.MinimumArity, symbol.MaximumArity);

      foreach (var s in Symbols) {
        if (s == ProgramRootSymbol) continue;
        if (s.MaximumArity > 0) AddAllowedChildSymbol(s, symbol);

        if (s == DefunSymbol) continue;
        if (s == StartSymbol) continue;
        if (symbol.MaximumArity > 0) AddAllowedChildSymbol(symbol, s);
      }
    }
    public void AddSymbols(IEnumerable<string> symbolNames, int minimumArity, int maximumArity) {
      foreach (var symbolName in symbolNames) AddSymbol(symbolName, minimumArity, maximumArity);
    }

    public void AddTerminalSymbol(string symbolName) {
      AddTerminalSymbol(symbolName, string.Empty);
    }
    public void AddTerminalSymbol(string symbolName, string description) {
      AddSymbol(symbolName, description, 0, 0);
    }
    public void AddTerminalSymbols(IEnumerable<string> symbolNames) {
      foreach (var symbolName in symbolNames) AddTerminalSymbol(symbolName);
    }

  }
}
