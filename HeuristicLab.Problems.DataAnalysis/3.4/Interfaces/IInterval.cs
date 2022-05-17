using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HeuristicLab.Problems.DataAnalysis {
  public interface IInterval : IEquatable<IInterval>{
    double LowerBound { get; }
    double UpperBound { get; }
    double Width {get;}
    bool Contains(double value);
    bool Contains(IInterval interval);

  }
}
