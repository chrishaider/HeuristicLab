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

using System.Globalization;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  internal enum TokenSymbol {
    LPAR, RPAR, SYMB, CONSTANT,
    NUM, // num
    EQ,  // =
    LBRACKET, // <
    RBRACKET  // >
  };
  internal class Token {
    public static readonly Token LPAR = Token.Parse("(");
    public static readonly Token RPAR = Token.Parse(")");
    public static readonly Token LBRACKET = Token.Parse("<");
    public static readonly Token RBRACKET = Token.Parse(">");
    public static readonly Token EQ = Token.Parse("=");
    public static readonly Token NUM = Token.Parse("num");

    public TokenSymbol Symbol { get; set; }
    public string StringValue { get; set; }
    public double DoubleValue { get; set; }
    public Token() { }

    public override bool Equals(object obj) {
      Token other = (obj as Token);
      if (other == null) return false;
      if (other.Symbol != Symbol) return false;
      return other.StringValue == this.StringValue;
    }

    public override int GetHashCode() {
      return Symbol.GetHashCode() & StringValue.GetHashCode();
    }

    public static Token Parse(string strToken) {
      strToken = strToken.Trim();
      var t = new Token();
      t.StringValue = strToken.Trim();
      if (strToken == "") {
        t = null;
      } else if (strToken == "(") {
        t.Symbol = TokenSymbol.LPAR;
      } else if (strToken == ")") {
        t.Symbol = TokenSymbol.RPAR;
      } else if (strToken == "<") {
        t.Symbol = TokenSymbol.LBRACKET;
      } else if (strToken == ">") {
        t.Symbol = TokenSymbol.RBRACKET;
      } else if (strToken == "=") {
        t.Symbol = TokenSymbol.EQ;
      } else if (strToken.ToLower() == "num") {
        t.Symbol = TokenSymbol.NUM;
      } else if (double.TryParse(strToken, NumberStyles.Float, CultureInfo.InvariantCulture.NumberFormat, out var val)) {
        t.Symbol = TokenSymbol.CONSTANT;
        t.DoubleValue = val;
      } else {
        t.Symbol = TokenSymbol.SYMB;
        t.StringValue = t.StringValue.ToUpper();
      }
      return t;
    }
  }
}
