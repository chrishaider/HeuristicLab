using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;

namespace HeuristicLab.Problems.DataAnalysis {
  public class BaseIntervalCalculator<T> where T : IInterval {
    #region Operations
    // [x1,x2] + [y1,y2] = [x1 + y1,x2 + y2]
    public static T Add(T x1, T x2) {
      return (T)Activator.CreateInstance(typeof(T), new object[] { x1.LowerBound + x2.LowerBound, x1.UpperBound + x2.UpperBound });
    }

    // [x1,x2] − [y1,y2] = [x1 − y2,x2 − y1]
    public static T Subtract(T x1, T x2) {
      return (T)Activator.CreateInstance(typeof(T), new object[] { x1.LowerBound - x2.UpperBound, x1.UpperBound - x2.LowerBound });
    }

    // [x1,x2] * [y1,y2] = [min(x1*y1,x1*y2,x2*y1,x2*y2),max(x1*y1,x1*y2,x2*y1,x2*y2)]
    public static T Multiply(T x1, T x2) {
      double v1 = x1.LowerBound * x2.LowerBound;
      double v2 = x1.LowerBound * x2.UpperBound;
      double v3 = x1.UpperBound * x2.LowerBound;
      double v4 = x1.UpperBound * x2.UpperBound;

      double min = Math.Min(Math.Min(v1, v2), Math.Min(v3, v4));
      double max = Math.Max(Math.Max(v1, v2), Math.Max(v3, v4));
      return (T)Activator.CreateInstance(typeof(T), new object[] { min, max });
    }

    //Division by intervals containing 0 is implemented as defined in
    //http://en.wikipedia.org/wiki/Interval_arithmetic
    public static T Divide(T x1, T x2) {
      if (x2.Contains(0.0)) {
        if (x2.LowerBound.IsAlmost(0.0)) return Multiply(x1, (T)Activator.CreateInstance(typeof(T), new object[] { 1.0 / x2.UpperBound, double.PositiveInfinity }));
        else if (x2.UpperBound.IsAlmost(0.0)) return Multiply(x1, (T)Activator.CreateInstance(typeof(T), new object[] { (double.NegativeInfinity, 1.0 / x2.LowerBound) }));
        else return (T)Activator.CreateInstance(typeof(T), new object[] { double.NegativeInfinity, double.PositiveInfinity });
      }
      return Multiply(x1, (T)Activator.CreateInstance(typeof(T), new object[] { 1.0 / x2.UpperBound, 1.0 / x2.LowerBound }));
    }

    public static T Sine(T x1) {
      if (Math.Abs(x1.UpperBound - x1.LowerBound) >= Math.PI * 2) return (T)Activator.CreateInstance(typeof(T), new object[] { -1, 1 });

      //divide the interval by PI/2 so that the optima lie at x element of N (0,1,2,3,4,...)
      double Pihalf = Math.PI / 2;
      var scaled = Divide(x1, (T)Activator.CreateInstance(typeof(T), new object[] { Pihalf, Pihalf }));
      //move to positive scale
      if (scaled.LowerBound < 0) {
        int periodsToMove = Math.Abs((int)scaled.LowerBound / 4) + 1;
        scaled = Add(scaled, (T)Activator.CreateInstance(typeof(T), new object[] { periodsToMove * 4, periodsToMove * 4 }));
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

      return (T)Activator.CreateInstance(typeof(T), new object[] { sinValues.Min(), sinValues.Max() });
    }

    public static T Cosine(T x1) {
      return Sine(Add(x1, (T)Activator.CreateInstance(typeof(T), new object[] { Math.PI / 2, Math.PI / 2 })));
    }

    public static T Tangens(T x1) {
      return Divide(Sine(x1), Cosine(x1));
    }

    public static T HyperbolicTangent(T x1) {
      return (T)Activator.CreateInstance(typeof(T), new object[] { Math.Tanh(x1.LowerBound), Math.Tanh(x1.UpperBound) });
    }

    public static T Logarithm(T x1) {
      return (T)Activator.CreateInstance(typeof(T), new object[] { Math.Log(x1.LowerBound), Math.Log(x1.UpperBound) });
    }

    public static T Exponential(T x1) {
      return (T)Activator.CreateInstance(typeof(T), new object[] { Math.Exp(x1.LowerBound), Math.Exp(x1.UpperBound) });
    }

    public static T Square(T x1) {
      if (x1.UpperBound <= 0) return (T)Activator.CreateInstance(typeof(T), new object[] { x1.UpperBound * x1.UpperBound, x1.LowerBound * x1.LowerBound });    // interval is negative
      else if (x1.LowerBound >= 0) return (T)Activator.CreateInstance(typeof(T), new object[] { x1.LowerBound * x1.LowerBound, x1.UpperBound * x1.UpperBound }); // interval is positive
      else return (T)Activator.CreateInstance(typeof(T), new object[] { 0, Math.Max(x1.LowerBound * x1.LowerBound, x1.UpperBound * x1.UpperBound) }); // interval goes over zero
    }

    public static T Cube(T x1) {
      return (T)Activator.CreateInstance(typeof(T), new object[] { Math.Pow(x1.LowerBound, 3), Math.Pow(x1.UpperBound, 3) });
    }

    //!TODO Power

    /// <summary>
    /// The interval contains both possible results of the calculated square root +-sqrt(x). That results in a wider
    /// interval, but it contains all possible solutions.
    /// </summary>
    /// <param name="a">Interval to build square root from.</param>
    /// <returns></returns>
    public static T SquareRoot(T x1) {
      if (x1.LowerBound < 0) return (T)Activator.CreateInstance(typeof(T), new object[] { double.NaN, double.NaN });
      return (T)Activator.CreateInstance(typeof(T), new object[] { -Math.Sqrt(x1.UpperBound), Math.Sqrt(x1.UpperBound) });
    }

    public static T CubicRoot(T x1) {
      var lower = (x1.LowerBound < 0) ? -Math.Pow(-x1.LowerBound, 1d / 3d) : Math.Pow(x1.LowerBound, 1d / 3d);
      var upper = (x1.UpperBound < 0) ? -Math.Pow(-x1.UpperBound, 1d / 3d) : Math.Pow(x1.UpperBound, 1d / 3d);

      return (T)Activator.CreateInstance(typeof(T), new object[] { lower, upper });
    }

    public static T Absolute(T x1) {
      var absLower = Math.Abs(x1.LowerBound);
      var absUpper = Math.Abs(x1.UpperBound);
      var min = Math.Min(absLower, absUpper);
      var max = Math.Max(absLower, absUpper);

      if (x1.Contains(0.0)) {
        min = 0.0;
      }

      return (T)Activator.CreateInstance(typeof(T), new object[] { min, max });
    }

    public static T AnalyticQuotient(T x1, T x2) {
      var dividend = x1;
      var divisor = Add(Square(x2), (T)Activator.CreateInstance(typeof(T), new object[] { 1.0, 1.0 }));
      divisor = SquareRoot(divisor);

      var quotient = Divide(dividend, divisor);
      return quotient;
    }
    #endregion
  }
}
