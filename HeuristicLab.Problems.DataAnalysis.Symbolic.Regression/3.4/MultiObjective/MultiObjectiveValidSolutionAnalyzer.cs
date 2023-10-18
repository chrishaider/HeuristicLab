using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [Item("MultiObjectiveValidSolutionAnalyzer", "An operator which analyzes how many solutions per generation are valid.")]
  [StorableType("3BD76FA0-6070-4507-A33B-D510642613FB")]
  public class MultiObjectiveValidSolutionAnalyzer : SymbolicDataAnalysisMultiObjectiveAnalyzer {
    private const string ValidSolutionsParamterName = "ValidSolutions";
    private const string ResultsParameterName = "Results";
    #region parameter properties
    public ILookupParameter<ResultCollection> ResultsParameter {
      get { return (ILookupParameter<ResultCollection>)Parameters["Results"]; }
    }
    public ValueLookupParameter<DataTable> ValidSolutionParameter {
      get { return (ValueLookupParameter<DataTable>)Parameters[ValidSolutionsParamterName]; }
    }


    #endregion

    #region Constructors
    [StorableConstructor]
    protected MultiObjectiveValidSolutionAnalyzer(StorableConstructorFlag _) : base(_) { }
    protected MultiObjectiveValidSolutionAnalyzer(MultiObjectiveValidSolutionAnalyzer original, Cloner cloner) : base(original, cloner) { }
    public MultiObjectiveValidSolutionAnalyzer() : base() {

      Parameters.Add(new ValueLookupParameter<IRandom>("Random", "A pseudo random number generator."));
      Parameters.Add(new ValueLookupParameter<DataTable>(ValidSolutionsParamterName, "The data table to store the valid solutions."));
    }
    #endregion

    public override IOperation Apply() {
      var qualities = Qualities.ToArray();
      var results = ResultsParameter.ActualValue;

      var validSolutionsTable = ValidSolutionParameter.ActualValue;
      // if the table was not created yet, we create it here
      if (validSolutionsTable == null) {
        validSolutionsTable = new DataTable("Valid Solutions Histogram");
        ValidSolutionParameter.ActualValue = validSolutionsTable;
      }

      DataTable validSolutions;
      if (results.ContainsKey(ValidSolutionsParamterName)) {
        validSolutions = (DataTable)results[ValidSolutionsParamterName].Value;
      } else {
        validSolutions = new DataTable("Valid Solutions");
        validSolutions.Rows.Add(new DataRow("Valid"));
        results.Add(new Result(ValidSolutionsParamterName, validSolutions));
      }

      double validSol = qualities.Count(quality => quality[quality.Length - 1] == 0);
      double qualityPercent = (validSol / qualities.Length) * 100;


      validSolutions.Rows["Valid"].Values.Add(qualityPercent);




      return base.Apply();
    }


    public override IDeepCloneable Clone(Cloner cloner) {
      return new MultiObjectiveValidSolutionAnalyzer(this, cloner);
    }
  }
}
