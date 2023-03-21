using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Common;
using Microsoft.Win32.SafeHandles;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("51961F7D-0728-416B-8B81-2AF850646C30")]
  public class ModalInterval : IEquatable<ModalInterval> {
    [Storable]
    public double LowerBound { get; private set; }
    [Storable]
    public double UpperBound { get; private set; }
    public bool IsProper => LowerBound < UpperBound;
    public bool IsInfOrUndefined => double.IsInfinity(LowerBound) || double.IsInfinity(UpperBound) || double.IsNaN(LowerBound) || double.IsNaN(UpperBound); 
    public ModalInterval ToPropper => new ModalInterval(Math.Min(LowerBound, UpperBound), Math.Max(LowerBound, UpperBound));
    public ModalInterval Dual => new ModalInterval(UpperBound, LowerBound);
    //a + (b-a)/2
    public double Mid => LowerBound + (UpperBound - LowerBound) / 2;
    public bool IsSubset(ModalInterval interval) {
      return this.LowerBound >= interval.LowerBound && this.UpperBound <= interval.UpperBound;
    }

    public bool IsMonotonic() {
      return this.IsSubset(new ModalInterval(0, double.PositiveInfinity)) ||this.IsSubset(new ModalInterval(double.NegativeInfinity, 0));
    }

    public int GetMonotonicSign() {
      if (this.IsSubset(new ModalInterval(double.NegativeInfinity, 0)))
        return -1;
      else if (this.IsSubset(new ModalInterval(0, double.PositiveInfinity)))
        return 1;
      else
        return 0;
    }

    public Interval ToInterval() {
      if (this.IsProper)
        return new Interval(this.LowerBound, this.UpperBound);

      var proper = this.ToPropper;
      return new Interval(proper.LowerBound, proper.UpperBound);

    }

    public ModalInterval(double lowerBound, double upperBound) {
      if (lowerBound.IsAlmost(upperBound)) {
        //If the bounds go over zero
        if (lowerBound <= 0 && upperBound >= 0) {
          lowerBound = 0.0;
          upperBound = 0.0;
          //Interval is negative
        } else if (upperBound < 0) {
          lowerBound = upperBound;
          //Interval is positive
        } else {
          upperBound = lowerBound;
        }
      }
      LowerBound = lowerBound;
      UpperBound = upperBound;
    }

    public ModalInterval(double v) : this(v, v) { }

    public static ModalInterval Add(ModalInterval a, ModalInterval b) {
      var lower = a.LowerBound + b.LowerBound;
      var upper = a.UpperBound + b.UpperBound;
      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Sub(ModalInterval a, ModalInterval b) {
      var lower = a.LowerBound - b.UpperBound;
      var upper = a.UpperBound - b.LowerBound;
      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Mul(ModalInterval a, ModalInterval b) {
      if (a.IsInfOrUndefined || b.IsInfOrUndefined)
        return new ModalInterval(double.NaN);

      var lower = 0.0;
      var upper = 0.0;
      if (a.LowerBound >= 0 && a.UpperBound >= 0) {
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          lower = a.LowerBound * b.LowerBound;
          upper = a.UpperBound * b.UpperBound;
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          lower = a.LowerBound * b.LowerBound;
          upper = a.LowerBound * b.UpperBound;
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          lower = a.UpperBound * b.LowerBound;
          upper = a.UpperBound * b.UpperBound;
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          lower = a.UpperBound * b.LowerBound;
          upper = a.LowerBound * b.UpperBound;
        }
      } else if (a.LowerBound >= 0 && a.UpperBound < 0) {
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          lower = a.LowerBound * b.LowerBound;
          upper = a.UpperBound * b.LowerBound;
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          lower = Math.Max(a.LowerBound * b.LowerBound, a.UpperBound * b.UpperBound);
          upper = Math.Min(a.LowerBound * b.UpperBound, a.UpperBound * b.LowerBound);
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          lower = 0.0;
          upper = 0.0;
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          lower = a.UpperBound * b.UpperBound;
          upper = a.LowerBound * b.UpperBound;
        }
      } else if (a.LowerBound < 0 && a.UpperBound >= 0) {
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          lower = a.LowerBound * b.UpperBound;
          upper = a.UpperBound * b.UpperBound;
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          lower = 0.0;
          upper = 0.0;
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          lower = Math.Min(a.LowerBound * b.UpperBound, a.UpperBound * b.LowerBound);
          upper = Math.Max(a.LowerBound * b.LowerBound, a.UpperBound * b.UpperBound);
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          lower = a.UpperBound * b.LowerBound;
          upper = a.LowerBound * b.LowerBound;
        }
      } else if (a.LowerBound < 0 && a.UpperBound < 0) {
        if (b.LowerBound >= 0 && b.UpperBound >= 0) {
          lower = a.LowerBound * b.UpperBound;
          upper = a.UpperBound * b.LowerBound;
        } else if (b.LowerBound >= 0 && b.UpperBound < 0) {
          lower = a.UpperBound * b.UpperBound;
          upper = a.UpperBound * b.LowerBound;
        } else if (b.LowerBound < 0 && b.UpperBound >= 0) {
          lower = a.LowerBound * b.UpperBound;
          upper = a.LowerBound * b.LowerBound;
        } else if (b.LowerBound < 0 && b.UpperBound < 0) {
          lower = a.UpperBound * b.UpperBound;
          upper = a.LowerBound * b.LowerBound;
        }
      }

      return new ModalInterval(lower, upper);
    }

    public static ModalInterval DivTrue(ModalInterval a, ModalInterval b) {
      var recip = Recip(b);
      return Mul(a, recip);
    }

    public static ModalInterval DivFloor(ModalInterval a, ModalInterval b) {
      var y =DivTrue(a, b);
      return new ModalInterval(Math.Floor(y.LowerBound), Math.Floor(y.UpperBound));
    }

    public static ModalInterval Sqrt(ModalInterval a) {
      var lower = (a.LowerBound >= 0) ? Math.Sqrt(a.LowerBound) : double.NaN;
      var upper = (a.UpperBound >= 0) ? Math.Sqrt(a.UpperBound) : double.NaN;

      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Pow(ModalInterval a, int n) {
      var lower = double.NaN;
      var upper = double.NaN;
      if (n == 0) {
        return new ModalInterval(1);
      } else if (n == 1) {
        return new ModalInterval(a.LowerBound, a.UpperBound);
      } else {
        //Odd exponent
        if (n % 2 == 1) { 
          lower = Math.Pow(a.LowerBound, n);
          upper = Math.Pow(a.UpperBound, n);
        } else {
          if (a.LowerBound >= 0 && a.UpperBound >= 0) {
            lower = Math.Pow(a.LowerBound, n);
            upper = Math.Pow(a.UpperBound, n);
          } else if (a.LowerBound < 0 && a.UpperBound < 0) {
            lower = Math.Pow(a.UpperBound, n);
            upper = Math.Pow(a.LowerBound, n);
          } else if (a.LowerBound < 0 && a.UpperBound >= 0) {
            lower = 0;
            upper = Math.Max(Math.Pow(a.LowerBound, n), Math.Pow(a.UpperBound, n));
          } else if (a.LowerBound >= 0 && a.UpperBound <0) {
            lower = Math.Max(Math.Pow(a.LowerBound, n), Math.Pow(a.UpperBound, n));
            upper = 0;
          }
        }
      }
      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Sqr(ModalInterval a) {
      return Pow(a, 2);
    }

    public static ModalInterval Recip(ModalInterval a) {
      var lower = a.LowerBound;
      var upper = a.UpperBound;

      if ((lower < 0 && 0 < upper) || (lower == 0 && upper == 0)){
        return new ModalInterval(double.NaN);
      } else if (lower == 0) {
        return new ModalInterval(1.0 / upper, double.PositiveInfinity);
      } else if (upper == 0) {
        return new ModalInterval(double.NegativeInfinity, 1.0 / lower);
      }
      return new ModalInterval(1.0/upper, 1.0/lower);
    }

    public static ModalInterval Min(ModalInterval a , ModalInterval b) {
      var lower = Math.Min(a.LowerBound, b.LowerBound);
      var upper = Math.Min(a.UpperBound, b.UpperBound);
      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Max(ModalInterval a, ModalInterval b) {
      var lower = Math.Max(a.LowerBound, b.LowerBound);
      var upper = Math.Max(a.UpperBound, b.UpperBound);
      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Exp(ModalInterval a) {
      var lower = Math.Exp(a.LowerBound);
      var upper = Math.Exp(a.UpperBound);

      return new ModalInterval(lower, upper);
    }

    public static ModalInterval Log(ModalInterval a) {
      if (a.LowerBound <= 0 || a.UpperBound <= 0) {
        return new ModalInterval(double.NaN);
      }
      var lower = Math.Log(a.LowerBound);
      var upper = Math.Log(a.UpperBound);

      return new ModalInterval(lower, upper); 
    }

    public static ModalInterval Sin(ModalInterval a) {
      if (Math.Abs(a.UpperBound - a.LowerBound) >= Math.PI * 2) return new ModalInterval(-1, 1);

      //divide the interval by PI/2 so that the optima lie at x element of N (0,1,2,3,4,...)
      double Pihalf = Math.PI / 2;
      ModalInterval scaled = ModalInterval.DivTrue(a, new ModalInterval(Pihalf, Pihalf));
      //move to positive scale
      if (scaled.LowerBound < 0) {
        int periodsToMove = Math.Abs((int)scaled.LowerBound / 4) + 1;
        scaled = ModalInterval.Add(scaled, new ModalInterval(periodsToMove * 4, periodsToMove * 4));
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

    public static ModalInterval Cos(ModalInterval a) {
      return ModalInterval.Sin(ModalInterval.Add(a, new ModalInterval(Math.PI / 2, Math.PI / 2)));
    }
    
    public static ModalInterval Tan(ModalInterval a) {
      return ModalInterval.DivTrue(ModalInterval.Sin(a), ModalInterval.Cos(a));
    }
    
    public static ModalInterval Tanh(ModalInterval a) {
      return new ModalInterval(Math.Tanh(a.LowerBound), Math.Tanh(a.UpperBound));
    }

    public static ModalInterval Abs(ModalInterval a) {
      var res = a;
      if (!a.IsProper)
        res = a.Dual;

      var lower = Math.Abs(res.LowerBound);
      var upper = Math.Abs(res.UpperBound);

      res = new ModalInterval(lower, upper);
      if (!res.IsProper)
        res = res.Dual;

      return res;
    }

    public static ModalInterval AQ(ModalInterval a, ModalInterval b) {
      var dividend = a;
      var divisor = Add(Sqr(b), new ModalInterval(1));
      divisor = Sqrt(divisor);

      var quotient = DivTrue(dividend, divisor);
      return quotient;
    }

    public override string ToString() {
      return $"KaucherInterval: [{LowerBound}, {UpperBound}]";
    }

    public bool Equals(ModalInterval other) {
      if (other == null)
        return false;

      return (UpperBound.IsAlmost(other.UpperBound) || (double.IsNaN(UpperBound) && double.IsNaN(other.UpperBound))) &&
              (LowerBound.IsAlmost(other.LowerBound) || (double.IsNaN(LowerBound) && double.IsNaN(other.LowerBound)));
    }

    public override bool Equals(object obj) {
      return Equals(obj as ModalInterval);
    }

    public override int GetHashCode() {
      return LowerBound.GetHashCode() ^ UpperBound.GetHashCode();
    }

    public static ModalInterval operator +(ModalInterval a, ModalInterval b) => Add(a, b);
    public static ModalInterval operator +(double a, ModalInterval b) => Add(new ModalInterval(a), b);
    public static ModalInterval operator +(ModalInterval a, double b) => Add(a, new ModalInterval(b));
    public static ModalInterval operator -(ModalInterval a, ModalInterval b) => Sub(a, b);
    public static ModalInterval operator -(double a, ModalInterval b) => Sub(new ModalInterval(a), b);
    public static ModalInterval operator -(ModalInterval a, double b) => Sub(a, new ModalInterval(b));
    public static ModalInterval operator -(ModalInterval a) => Sub(new ModalInterval(0), a);
    public static ModalInterval operator *(ModalInterval a, ModalInterval b) => Mul(a, b);
    public static ModalInterval operator *(double a, ModalInterval b) => Mul(new ModalInterval(a), b);
    public static ModalInterval operator *(ModalInterval a, double b) => Mul(a, new ModalInterval(b));
    public static ModalInterval operator /(ModalInterval a, ModalInterval b) => DivTrue(a, b);
    public static ModalInterval operator /(double a, ModalInterval b) => DivTrue(new ModalInterval(a), b);
    public static ModalInterval operator /(ModalInterval a, double b) => DivTrue(a, new ModalInterval(b));

    public static ModalInterval Log(double a) => Log(new ModalInterval(a));
    public static ModalInterval Exp(double a) => Exp(new ModalInterval(a));
    public static ModalInterval Sqr(double a) => Sqr(new ModalInterval(a));
    public static ModalInterval Sqrt(double a) => Sqrt(new ModalInterval(a));
  }
}
