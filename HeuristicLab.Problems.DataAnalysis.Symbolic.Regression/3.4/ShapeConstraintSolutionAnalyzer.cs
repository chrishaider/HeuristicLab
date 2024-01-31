using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Analysis;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic.Regression {
  [StorableType("CF2E7064-863E-49C4-A6D9-10062B677146")]
  [Item("ShapeConstraintSolutionAnalyzer", "Analyzes the shape constraint solution.")]
  public class ShapeConstraintSolutionAnalyzer : SymbolicDataAnalysisAnalyzer, ISymbolicExpressionTreeAnalyzer {
    private const string ProblemDataParameterName = "ProblemData";
    private const string ConstraintViolationsParameterName = "ConstraintViolations";
    private const string InfeasibleSolutionsParameterName = "InfeasibleSolutions";
    private const string QualityParameterName = "Quality";
    //private const string SymbolicExpressionTreeParameterName = "SymbolicExpressionTree";
    //private const string ResultCollectionParameterName = "Results";

    #region parameter properties

    public ILookupParameter<IRegressionProblemData> RegressionProblemDataParameter =>
      (ILookupParameter<IRegressionProblemData>)Parameters[ProblemDataParameterName];

    public IResultParameter<DataTable> ConstraintViolationsParameter =>
      (IResultParameter<DataTable>)Parameters[ConstraintViolationsParameterName];

    public IResultParameter<DataTable> InfeasibleSolutionsParameter =>
      (IResultParameter<DataTable>)Parameters[InfeasibleSolutionsParameterName];

    public IScopeTreeLookupParameter<DoubleValue> QualityParameter => 
      (IScopeTreeLookupParameter<DoubleValue>)Parameters[QualityParameterName];

    /*public IScopeTreeLookupParameter<ISymbolicExpressionTree> SymblicExpressionTreeParameter =>
      (IScopeTreeLookupParameter<ISymbolicExpressionTree>)Parameters[SymbolicExpressionTreeParameterName];

    public ILookupParameter<ResultCollection> ResultCollectionParameter =>
      (ILookupParameter<ResultCollection>)Parameters[ResultCollectionParameterName];*/

    #endregion

    #region properties
    public IRegressionProblemData RegressionProblemData => RegressionProblemDataParameter.ActualValue;
    public DataTable ConstraintViolations => ConstraintViolationsParameter.ActualValue;
    public DataTable InfeasibleSolutions => InfeasibleSolutionsParameter.ActualValue;
    public ItemArray<DoubleValue> Quality => QualityParameter.ActualValue;
    //public ItemArray<ISymbolicExpressionTree> SymbolicExpressionTree => SymbolicExpressionTreeParameter.ActualValue;
    //public ResultCollection ResultCollection => ResultCollectionParameter.ActualValue;
    #endregion

    public override bool EnabledByDefault => false;

    #region Constructors

    [StorableConstructor]
    protected ShapeConstraintSolutionAnalyzer(StorableConstructorFlag _) : base(_) { }

    protected ShapeConstraintSolutionAnalyzer(ShapeConstraintSolutionAnalyzer original, Cloner cloner) :
      base(original, cloner) { }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new ShapeConstraintSolutionAnalyzer(this, cloner);
    }

    public ShapeConstraintSolutionAnalyzer() {
      Parameters.Add(new LookupParameter<IRegressionProblemData>(ProblemDataParameterName,
        "The problem data of the symbolic data analysis problem."));
      Parameters.Add(new ScopeTreeLookupParameter<DoubleValue>(QualityParameterName, 
        "The qualities of the trees that should be analyzed."));
      /*Parameters.Add(new ScopeTreeLookupParameter<ISymbolicExpressionTree>(SymbolicExpressionTreeParameterName, 
        "The symbolic expression trees that should be analyzed."));*/
     /* Parameters.Add(new LookupParameter<ResultCollection>(ResultCollectionParameterName, 
        "The result collection to store the analysis results."));*/
    }

    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() { }

    #endregion


    public override IOperation Apply() {
      var problemData = (IShapeConstrainedRegressionProblemData)RegressionProblemData;
      double[] trainingQuality = QualityParameter.ActualValue.Select(x => x.Value).ToArray();
      ISymbolicExpressionTree[] trees = SymbolicExpressionTreeParameter.ActualValue.Select(x => x).ToArray();
      return base.Apply();
    }
  }
}
