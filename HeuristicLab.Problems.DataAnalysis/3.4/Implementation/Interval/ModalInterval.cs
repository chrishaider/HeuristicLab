using System;
using System.Collections.Generic;
using System.Linq;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("FE9BA41B-6379-45BF-B701-F27A1EBE11B4")]
  public readonly struct ModalInterval : IEquatable<ModalInterval> {
    #region props
    [Storable]
    public double LowerBound { get; }
    [Storable]
    public double UpperBound { get; }
    public double Width => Math.Abs(LowerBound - UpperBound);
    public bool isProper => LowerBound <= UpperBound;
    #endregion

    #region cstr
    public ModalInterval(double lowerBound, double upperBound) {
      LowerBound = lowerBound;
      UpperBound = upperBound;
    }

    public ModalInterval(double v) : this(v,v) { }
    #endregion

    #region operations
    public static ModalInterval Add(ModalInterval a, ModalInterval b) {
      return new ModalInterval(a.LowerBound + b.LowerBound, a.UpperBound + b.UpperBound);
    }

    public static ModalInterval Subtract(ModalInterval a, ModalInterval b) {
      return new ModalInterval(a.LowerBound - b.UpperBound, a.UpperBound - b.LowerBound);
    }

    public static ModalInterval Sine(ModalInterval a) {
      if (Math.Abs(a.UpperBound - a.LowerBound) >= Math.PI * 2) return new ModalInterval(-1, 1);

      //divide the interval by PI/2 so that the optima lie at x element of N (0,1,2,3,4,...)
      double Pihalf = Math.PI / 2;
      ModalInterval scaled = ModalInterval.Divide(a, new ModalInterval(Pihalf, Pihalf));
      //move to positive scale
      if (scaled.LowerBound < 0) {
        int periodsToMove = Math.Abs((int)scaled.LowerBound / 4) + 1;
        scaled = Add(scaled, new ModalInterval(periodsToMove * 4, periodsToMove * 4));
      }

      double scaledLowerBound = scaled.LowerBound % 4.0;
      double scaledUpperBound = scaled.UpperBound % 4.0;
      if (scaledUpperBound < scaledLowerBound) scaledUpperBound += 4.0;
      List<double> sinValues = new List<double>();
      sinValues.Add(Math.Sin(scaledLowerBound * Pihalf));
      sinValues.Add(Math.Sin(scaledUpperBound * Pihalf));

      int startValue = (int)Math.Ceiling(scaledLowerBound);
      while (startValue < scaledUpperBound) {
        sinValues.Add(Math.Sin(startValue * Pihalf));
        startValue += 1;
      }

      return new ModalInterval(sinValues.Min(), sinValues.Max());
    }

    public static ModalInterval Cosine(ModalInterval a) {
      return Sine(Add(a, new ModalInterval(Math.PI / 2, Math.PI / 2)));
    }
    public static ModalInterval Tangens(ModalInterval a) {
      return Divide(Sine(a), Cosine(a));
    }
    public static ModalInterval HyperbolicTangent(ModalInterval a) {
      return new ModalInterval(Math.Tanh(a.LowerBound), Math.Tanh(a.UpperBound));
    }

    public static ModalInterval Logarithm(ModalInterval a) {
      return new ModalInterval(Math.Log(a.LowerBound), Math.Log(a.UpperBound));
    }

    public static ModalInterval Exponential(ModalInterval a) {
      return new ModalInterval(Math.Exp(a.LowerBound), Math.Exp(a.UpperBound));
    }

    public static ModalInterval Square(ModalInterval a) {
      return ModalInterval.Power(a, 2);
    }

    public static ModalInterval Cube(ModalInterval a) {
      return ModalInterval.Power(a, 3);
    }

    public static ModalInterval SquareRoot(ModalInterval a) {
      if (a.LowerBound < 0) return new ModalInterval(double.NaN, double.NaN);
      return new ModalInterval(-Math.Sqrt(a.UpperBound), Math.Sqrt(a.UpperBound));
    }

    public static ModalInterval CubicRoot(ModalInterval a) {
      var lower = (a.LowerBound < 0) ? -Math.Pow(-a.LowerBound, 1d / 3d) : Math.Pow(a.LowerBound, 1d / 3d);
      var upper = (a.UpperBound < 0) ? -Math.Pow(-a.UpperBound, 1d / 3d) : Math.Pow(a.UpperBound, 1d / 3d);

      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Power(ModalInterval a, int k) {
      //special treatment for power 0 and 1
      if (k == 0) {
        return new ModalInterval(1);
      }
      if (k == 1) {
        return a;
      }

      //logic for power 
      if (k % 2 != 0) {
        return new ModalInterval(Math.Pow(a.LowerBound, k), Math.Pow(a.UpperBound, k));
      } else if (a.LowerBound >= 0 && a.UpperBound >= 0) {
        return new ModalInterval(Math.Pow(a.LowerBound, k), Math.Pow(a.UpperBound, k));
      } else if (a.LowerBound < 0 && a.UpperBound < 0) {
        return new ModalInterval(Math.Pow(a.UpperBound, k), Math.Pow(a.LowerBound, k));
      } else if (a.LowerBound < 0 && a.UpperBound >= 0) {
        return new ModalInterval(0, Math.Max(Math.Pow(a.LowerBound, k), Math.Pow(a.UpperBound, k)));
      } else if (a.LowerBound >= 0 && a.UpperBound < 0) {
        return new ModalInterval(Math.Max(Math.Pow(a.LowerBound, k), Math.Pow(a.UpperBound, k)), 0);
      }

      //fallback
      return new ModalInterval(double.NaN, double.NaN);
    }

    public static ModalInterval Multiply(ModalInterval a, ModalInterval b) {
      if (a.LowerBound >= 0 && a.UpperBound >= 0) {
        //case1mul
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          return new ModalInterval(a.LowerBound * b.LowerBound, a.UpperBound * b.UpperBound);
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          return new ModalInterval(a.LowerBound * b.LowerBound, a.LowerBound * b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          return new ModalInterval(a.UpperBound * b.LowerBound, a.UpperBound * b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound * b.LowerBound, a.LowerBound * b.UpperBound);
        }
      } else if (a.LowerBound >= 0 && a.UpperBound < 0) {
        //case2mul
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          return new ModalInterval(a.LowerBound * b.LowerBound, a.UpperBound * b.LowerBound);
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          return new ModalInterval(Math.Max(a.LowerBound * b.LowerBound, a.UpperBound * b.UpperBound), Math.Min(a.LowerBound * b.UpperBound, a.UpperBound * b.LowerBound));
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          return new ModalInterval(0);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound * b.UpperBound, a.LowerBound * b.LowerBound);
        }
      } else if (a.LowerBound < 0 && a.UpperBound >= 0) {
        //case3mul
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          return new ModalInterval(a.LowerBound * b.UpperBound, a.UpperBound * b.UpperBound);
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          return new ModalInterval(0);
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          return new ModalInterval(Math.Min(a.LowerBound * b.UpperBound, a.UpperBound * b.LowerBound), Math.Max(a.LowerBound * b.LowerBound, a.UpperBound * b.UpperBound));
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound * b.LowerBound, a.LowerBound * b.LowerBound);
        }
      } else {
        //case4mul
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          return new ModalInterval(a.LowerBound * b.UpperBound, a.UpperBound * b.LowerBound);
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound * b.UpperBound, a.UpperBound * b.LowerBound);
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          return new ModalInterval(a.LowerBound * b.UpperBound, a.LowerBound * b.LowerBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound * b.UpperBound, a.LowerBound * b.LowerBound);
        }
      }

      //fallback
      return new ModalInterval(double.NaN, double.NaN);
    }

    public static ModalInterval Absolute(ModalInterval a) {
      if (a.LowerBound >= 0 && a.UpperBound >= 0) {
        return new ModalInterval(a.LowerBound, a.UpperBound);
      } else if (a.LowerBound < 0 && a.UpperBound < 0) {
        return new ModalInterval(a.UpperBound, a.LowerBound);
      } else if (a.LowerBound < 0 && a.UpperBound >= 0) {
        return new ModalInterval(0, Math.Max(-a.LowerBound, a.UpperBound));
      } else if (a.LowerBound >= 0 && a.UpperBound < 0) {
        return new ModalInterval(Math.Max(a.LowerBound, -a.UpperBound), 0);
      }

      //fallback
      return a;
    }

    public static ModalInterval Divide(ModalInterval a, ModalInterval b) {
      if ((b.LowerBound > 0 && b.UpperBound < 0) ||
          (b.LowerBound < 0 && b.UpperBound > 0) ||
          (b.LowerBound == 0 && b.UpperBound == 0) ||
          ToProper(b).Contains(0)) {
        return new ModalInterval(double.NaN);
      }

      if (a.LowerBound >= 0 && a.UpperBound >= 0) {
        //case1div
        if (b.LowerBound > 0 && b.UpperBound > 0) {
          return new ModalInterval(a.LowerBound / b.UpperBound, a.UpperBound / b.LowerBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound / b.UpperBound, a.LowerBound / b.LowerBound);
        } else if (b.LowerBound > 0 && b.UpperBound == 0) {
          return new ModalInterval(double.PositiveInfinity, a.UpperBound / b.LowerBound);
        } else if (b.LowerBound == 0 && b.UpperBound > 0) {
          return new ModalInterval(a.LowerBound / b.UpperBound, double.PositiveInfinity);
        } else if (b.LowerBound < 0 && b.UpperBound == 0) {
          return new ModalInterval(double.NegativeInfinity, a.LowerBound / b.LowerBound);
        } else if (b.LowerBound == 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound / b.UpperBound, double.NegativeInfinity);
        }
      } else if (a.LowerBound >= 0 && a.UpperBound < 0) {
        //case2div
        if (b.LowerBound > 0 && b.UpperBound > 0) {
          return new ModalInterval(a.LowerBound / b.UpperBound, a.UpperBound / b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound / b.LowerBound, a.LowerBound / b.LowerBound);
        } else if (b.LowerBound > 0 && b.UpperBound == 0) {
          return new ModalInterval(double.PositiveInfinity, double.NegativeInfinity);
        } else if (b.LowerBound == 0 && b.UpperBound > 0) {
          return new ModalInterval(a.LowerBound / b.UpperBound, a.UpperBound / b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound == 0) {
          return new ModalInterval(a.UpperBound / b.LowerBound, a.LowerBound / b.LowerBound);
        } else if (b.LowerBound == 0 && b.UpperBound < 0) {
          return new ModalInterval(double.PositiveInfinity, double.NegativeInfinity);
        }
      } else if (a.LowerBound < 0 && a.UpperBound >= 0) {
        //case3div
        if (b.LowerBound > 0 && b.UpperBound > 0) {
          return new ModalInterval(a.LowerBound / b.LowerBound, a.UpperBound / b.LowerBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound / b.UpperBound, a.LowerBound / b.UpperBound);
        } else if (b.LowerBound > 0 && b.UpperBound == 0) {
          return new ModalInterval(a.LowerBound / b.LowerBound, a.UpperBound / b.LowerBound);
        } else if (b.LowerBound == 0 && b.UpperBound > 0) {
          return new ModalInterval(double.NegativeInfinity, double.PositiveInfinity);
        } else if (b.LowerBound < 0 && b.UpperBound == 0) {
          return new ModalInterval(double.NegativeInfinity, double.PositiveInfinity);
        } else if (b.LowerBound == 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound / b.UpperBound, a.LowerBound / b.UpperBound);
        }
      } else if (a.LowerBound < 0 && a.UpperBound < 0) {
        //case4div
        if (b.LowerBound > 0 && b.UpperBound > 0) {
          return new ModalInterval(a.LowerBound / b.LowerBound, a.UpperBound / b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          return new ModalInterval(a.UpperBound / b.LowerBound, a.LowerBound / b.UpperBound);
        } else if (b.LowerBound > 0 && b.UpperBound == 0) {
          return new ModalInterval(a.LowerBound / b.LowerBound, double.NegativeInfinity);
        } else if (b.LowerBound == 0 && b.UpperBound > 0) {
          return new ModalInterval(double.NegativeInfinity, a.UpperBound / b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound == 0) {
          return new ModalInterval(a.UpperBound / b.LowerBound, double.PositiveInfinity);
        } else if (b.LowerBound == 0 && b.UpperBound < 0) {
          return new ModalInterval(double.PositiveInfinity, a.LowerBound / b.UpperBound);
        }
      }

      //fallback
      return new ModalInterval(double.NaN);
    }

    public static ModalInterval AnalyticQuotient(ModalInterval a, ModalInterval b) {
      var dividend = a;
      var divisor = Add(Square(b), new ModalInterval(1.0, 1.0));
      divisor = SquareRoot(divisor);

      var quotient = Divide(dividend, divisor);
      return quotient;
    }
    #endregion

    #region op overloads
    public static ModalInterval operator +(ModalInterval a, ModalInterval b) => Add(a, b);
    public static ModalInterval operator +(double a, ModalInterval b) => Add(new ModalInterval(a), b);
    public static ModalInterval operator +(ModalInterval a, double b) => Add(a, new ModalInterval(b));
    public static ModalInterval operator -(ModalInterval a, ModalInterval b) => Subtract(a, b);
    public static ModalInterval operator -(ModalInterval a, double b) => Subtract(a, new ModalInterval(b));
    public static ModalInterval operator -(double a, ModalInterval b) => Subtract(new ModalInterval(a), b);
    public static ModalInterval operator -(ModalInterval a) => Subtract(new ModalInterval(0), a);
    public static ModalInterval operator *(ModalInterval a, ModalInterval b) => Multiply(a, b);
    public static ModalInterval operator *(ModalInterval a, double b) => Multiply(a, new ModalInterval(b));
    public static ModalInterval operator *(double a, ModalInterval b) => Multiply(new ModalInterval(a), b);
    public static ModalInterval operator /(ModalInterval a, ModalInterval b) => Divide(a, b);
    public static ModalInterval operator /(ModalInterval a, double b) => Divide(a, new ModalInterval(b));
    public static ModalInterval operator /(double a, ModalInterval b) => Divide(new ModalInterval(a), b);
    public static ModalInterval Exponential(double a) { return Exponential(new ModalInterval(a)); }
    public static ModalInterval Logarithm(double a) { return Logarithm(new ModalInterval(a)); }
    public static ModalInterval Sine(double a) { return Sine(new ModalInterval(a)); }
    public static ModalInterval Cosine(double a) { return Cosine(new ModalInterval(a)); }
    public static ModalInterval Tangens(double a) { return Tangens(new ModalInterval(a)); }
    public static ModalInterval HyperbolicTangent(double a) { return HyperbolicTangent(new ModalInterval(a)); }
    public static ModalInterval Square(double a) { return Square(new ModalInterval(a)); }
    public static ModalInterval Cube(double a) { return Cube(new ModalInterval(a)); }
    public static ModalInterval SquareRoot(double a) { return SquareRoot(new ModalInterval(a)); }
    public static ModalInterval CubicRoot(double a) { return CubicRoot(new ModalInterval(a)); }
    public static ModalInterval Absolute(double a) { return Absolute(new ModalInterval(a)); }
    public static ModalInterval AnalyticQuotient(ModalInterval a, double b) { return AnalyticQuotient(a, new ModalInterval(b)); }
    public static ModalInterval AnalyticQuotient(double a, ModalInterval b) { return AnalyticQuotient(new ModalInterval(a), b); }
    public static ModalInterval AnalyticQuotient(double a, double b) { return AnalyticQuotient(new ModalInterval(a), new ModalInterval(b)); }

    #endregion

    #region helpers
    public bool Contains(double value) => value >= Math.Min(LowerBound, UpperBound) && value <= Math.Max(UpperBound, LowerBound);

    public ModalInterval ToImproper(ModalInterval a) {
      if (a.LowerBound > a.UpperBound) return a;
      return new ModalInterval(a.UpperBound, a.LowerBound);
    }

    public static ModalInterval ToProper(ModalInterval a) {
      if (a.LowerBound <= a.UpperBound) return a;
      return new ModalInterval(a.UpperBound, a.LowerBound);
    }

    public static ModalInterval Dual(ModalInterval a) => new ModalInterval(a.UpperBound, a.LowerBound);

    public static ModalInterval Mid(ModalInterval a) {
      var mid = a.LowerBound + (a.UpperBound - a.LowerBound) / 2;
      return new ModalInterval(mid);
    }
    #endregion

    #region Equals, Hash
    public bool Equals(ModalInterval other) {
      if (other == null)
        return false;

      return (UpperBound == other.UpperBound || (double.IsNaN(UpperBound) && double.IsNaN(other.UpperBound)))
        && (LowerBound == other.LowerBound || (double.IsNaN(LowerBound) && double.IsNaN(other.LowerBound)));
    }

    public override bool Equals(object obj) {
      if (obj is ModalInterval modal) {
        return LowerBound == modal.LowerBound && UpperBound == modal.UpperBound;
      }
      return false;
    }

    public override int GetHashCode() {
      return LowerBound.GetHashCode() ^ UpperBound.GetHashCode();
    }

    public static bool operator ==(ModalInterval left, ModalInterval right) {
      if (ReferenceEquals(left, null)) return ReferenceEquals(right, null);
      return left.Equals(right);
    }

    public static bool operator !=(ModalInterval left, ModalInterval right) {
      return !(left == right);
    }

    #endregion
  }
}
