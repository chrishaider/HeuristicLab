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

extern alias alglib_3_7;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.DataAnalysis;
using HeuristicLab.Random;

namespace HeuristicLab.Algorithms.DataAnalysis {
  [Item("RFParameter", "A random forest parameter collection")]
  [StorableType("40E482DA-63C5-4D39-97C7-63701CF1D021")]
  public class RFParameter : ParameterCollection {
    public RFParameter() {
      base.Add(new FixedValueParameter<IntValue>("N", "The number of random forest trees", new IntValue(50)));
      base.Add(new FixedValueParameter<DoubleValue>("M", "The ratio of features that will be used in the construction of individual trees (0<m<=1)", new DoubleValue(0.1)));
      base.Add(new FixedValueParameter<DoubleValue>("R", "The ratio of the training set that will be used in the construction of individual trees (0<r<=1)", new DoubleValue(0.1)));
    }

    [StorableConstructor]
    protected RFParameter(StorableConstructorFlag _) : base(_) {
    }

    protected RFParameter(RFParameter original, Cloner cloner)
      : base(original, cloner) {
      this.N = original.N;
      this.R = original.R;
      this.M = original.M;
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new RFParameter(this, cloner);
    }

    private IFixedValueParameter<IntValue> NParameter {
      get { return (IFixedValueParameter<IntValue>)base["N"]; }
    }

    private IFixedValueParameter<DoubleValue> RParameter {
      get { return (IFixedValueParameter<DoubleValue>)base["R"]; }
    }

    private IFixedValueParameter<DoubleValue> MParameter {
      get { return (IFixedValueParameter<DoubleValue>)base["M"]; }
    }

    public int N {
      get { return NParameter.Value.Value; }
      set { NParameter.Value.Value = value; }
    }

    public double R {
      get { return RParameter.Value.Value; }
      set { RParameter.Value.Value = value; }
    }

    public double M {
      get { return MParameter.Value.Value; }
      set { MParameter.Value.Value = value; }
    }
  }

  public static class RandomForestUtil {
    public static void AssertParameters(double r, double m) {
      if (r <= 0 || r > 1) throw new ArgumentException("The R parameter for random forest modeling must be between 0 and 1.");
      if (m <= 0 || m > 1) throw new ArgumentException("The M parameter for random forest modeling must be between 0 and 1.");
    }

    public static void AssertInputMatrix(double[,] inputMatrix) {
      foreach(var val in inputMatrix) if(double.IsNaN(val))
        throw new NotSupportedException("Random forest modeling does not support NaN values in the input dataset.");
    }

    internal static alglib.decisionforest CreateRandomForestModel(int seed, double[,] inputMatrix, int nTrees, double r, double m, int nClasses, out alglib.dfreport rep) {
      RandomForestUtil.AssertParameters(r, m);
      RandomForestUtil.AssertInputMatrix(inputMatrix);

      int nRows = inputMatrix.GetLength(0);
      int nColumns = inputMatrix.GetLength(1);

      alglib.dfbuildercreate(out var dfbuilder);
      alglib.dfbuildersetdataset(dfbuilder, inputMatrix, nRows, nColumns - 1, nClasses);
      alglib.dfbuildersetimportancenone(dfbuilder); // do not calculate importance (TODO add this feature)
      alglib.dfbuildersetrdfalgo(dfbuilder, 0); // only one algorithm supported in version 3.17
      alglib.dfbuildersetrdfsplitstrength(dfbuilder, 2); // 0 = split at the random position, fastest one
                                                         // 1 = split at the middle of the range
                                                         // 2 = strong split at the best point of the range (default)
      alglib.dfbuildersetrndvarsratio(dfbuilder, m);
      alglib.dfbuildersetsubsampleratio(dfbuilder, r);
      alglib.dfbuildersetseed(dfbuilder, seed);
      alglib.dfbuilderbuildrandomforest(dfbuilder, nTrees, out var dForest, out rep);
      return dForest;
    }
    internal static alglib_3_7.alglib.decisionforest CreateRandomForestModelAlglib_3_7(int seed, double[,] inputMatrix, int nTrees, double r, double m, int nClasses, out alglib_3_7.alglib.dfreport rep) {
      RandomForestUtil.AssertParameters(r, m);
      RandomForestUtil.AssertInputMatrix(inputMatrix);

      int info = 0;
      alglib_3_7.alglib.math.rndobject = new System.Random(seed);
      var dForest = new alglib_3_7.alglib.decisionforest();
      rep = new alglib_3_7.alglib.dfreport();
      int nRows = inputMatrix.GetLength(0);
      int nColumns = inputMatrix.GetLength(1);
      int sampleSize = Math.Max((int)Math.Round(r * nRows), 1);
      int nFeatures = Math.Max((int)Math.Round(m * (nColumns - 1)), 1);

      alglib_3_7.alglib.dforest.dfbuildinternal(inputMatrix, nRows, nColumns - 1, nClasses, nTrees, sampleSize, nFeatures, alglib_3_7.alglib.dforest.dfusestrongsplits + alglib_3_7.alglib.dforest.dfuseevs, ref info, dForest.innerobj, rep.innerobj);
      if (info != 1) throw new ArgumentException("Error in calculation of random forest model");
      return dForest;
    }


    private static void CrossValidate(IRegressionProblemData problemData, Tuple<IEnumerable<int>, IEnumerable<int>>[] partitions, int nTrees, double r, double m, int seed, out double avgTestMse) {
      avgTestMse = 0;
      var ds = problemData.Dataset;
      var targetVariable = GetTargetVariableName(problemData);
      foreach (var tuple in partitions) {
        var trainingRandomForestPartition = tuple.Item1;
        var testRandomForestPartition = tuple.Item2;
        var model = RandomForestRegression.CreateRandomForestRegressionModel(problemData, trainingRandomForestPartition, nTrees, r, m, seed,
                                                                             out var rmsError, out var avgRelError, out var outOfBagRmsError, out var outOfBagAvgRelError);
        var estimatedValues = model.GetEstimatedValues(ds, testRandomForestPartition);
        var targetValues = ds.GetDoubleValues(targetVariable, testRandomForestPartition);
        OnlineCalculatorError calculatorError;
        double mse = OnlineMeanSquaredErrorCalculator.Calculate(estimatedValues, targetValues, out calculatorError);
        if (calculatorError != OnlineCalculatorError.None)
          mse = double.NaN;
        avgTestMse += mse;
      }
      avgTestMse /= partitions.Length;
    }

    private static void CrossValidate(IClassificationProblemData problemData, Tuple<IEnumerable<int>, IEnumerable<int>>[] partitions, int nTrees, double r, double m, int seed, out double avgTestAccuracy) {
      avgTestAccuracy = 0;
      var ds = problemData.Dataset;
      var targetVariable = GetTargetVariableName(problemData);
      foreach (var tuple in partitions) {
        var trainingRandomForestPartition = tuple.Item1;
        var testRandomForestPartition = tuple.Item2;
        var model = RandomForestClassification.CreateRandomForestClassificationModel(problemData, trainingRandomForestPartition, nTrees, r, m, seed,
                                                                                     out var rmsError, out var avgRelError, out var outOfBagRmsError, out var outOfBagAvgRelError);
        var estimatedValues = model.GetEstimatedClassValues(ds, testRandomForestPartition);
        var targetValues = ds.GetDoubleValues(targetVariable, testRandomForestPartition);
        OnlineCalculatorError calculatorError;
        double accuracy = OnlineAccuracyCalculator.Calculate(estimatedValues, targetValues, out calculatorError);
        if (calculatorError != OnlineCalculatorError.None)
          accuracy = double.NaN;
        avgTestAccuracy += accuracy;
      }
      avgTestAccuracy /= partitions.Length;
    }

    /// <summary>
    /// Grid search without crossvalidation (since for random forests the out-of-bag estimate is unbiased)
    /// </summary>
    /// <param name="problemData">The regression problem data</param>
    /// <param name="parameterRanges">The ranges for each parameter in the grid search</param>
    /// <param name="seed">The random seed (required by the random forest model)</param>
    /// <param name="maxDegreeOfParallelism">The maximum allowed number of threads (to parallelize the grid search)</param>
    public static RFParameter GridSearch(IRegressionProblemData problemData, Dictionary<string, IEnumerable<double>> parameterRanges, int seed = 12345, int maxDegreeOfParallelism = 1) {
      var setters = parameterRanges.Keys.Select(GenerateSetter).ToList();
      var crossProduct = parameterRanges.Values.CartesianProduct();
      double bestOutOfBagRmsError = double.MaxValue;
      RFParameter bestParameters = new RFParameter();

      var locker = new object();
      Parallel.ForEach(crossProduct, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, parameterCombination => {
        var parameterValues = parameterCombination.ToList();
        var parameters = new RFParameter();
        for (int i = 0; i < setters.Count; ++i) { setters[i](parameters, parameterValues[i]); }
        RandomForestRegression.CreateRandomForestRegressionModel(problemData, problemData.TrainingIndices, parameters.N, parameters.R, parameters.M, seed,
                                                                 out var rmsError, out var avgRelError, out var outOfBagRmsError, out var outOfBagAvgRelError);

        lock (locker) {
          if (bestOutOfBagRmsError > outOfBagRmsError) {
            bestOutOfBagRmsError = outOfBagRmsError;
            bestParameters = (RFParameter)parameters.Clone();
          }
        }
      });
      return bestParameters;
    }

    /// <summary>
    /// Grid search without crossvalidation (since for random forests the out-of-bag estimate is unbiased)
    /// </summary>
    /// <param name="problemData">The classification problem data</param>
    /// <param name="parameterRanges">The ranges for each parameter in the grid search</param>
    /// <param name="seed">The random seed (required by the random forest model)</param>
    /// <param name="maxDegreeOfParallelism">The maximum allowed number of threads (to parallelize the grid search)</param>
    public static RFParameter GridSearch(IClassificationProblemData problemData, Dictionary<string, IEnumerable<double>> parameterRanges, int seed = 12345, int maxDegreeOfParallelism = 1) {
      var setters = parameterRanges.Keys.Select(GenerateSetter).ToList();
      var crossProduct = parameterRanges.Values.CartesianProduct();

      double bestOutOfBagRmsError = double.MaxValue;
      RFParameter bestParameters = new RFParameter();

      var locker = new object();
      Parallel.ForEach(crossProduct, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, parameterCombination => {
        var parameterValues = parameterCombination.ToList();
        var parameters = new RFParameter();
        for (int i = 0; i < setters.Count; ++i) { setters[i](parameters, parameterValues[i]); }
        RandomForestClassification.CreateRandomForestClassificationModel(problemData, problemData.TrainingIndices, parameters.N, parameters.R, parameters.M, seed,
                                                                         out var rmsError, out var avgRelError, out var outOfBagRmsError, out var outOfBagAvgRelError);

        lock (locker) {
          if (bestOutOfBagRmsError > outOfBagRmsError) {
            bestOutOfBagRmsError = outOfBagRmsError;
            bestParameters = (RFParameter)parameters.Clone();
          }
        }
      });
      return bestParameters;
    }

    /// <summary>
    /// Grid search with crossvalidation
    /// </summary>
    /// <param name="problemData">The regression problem data</param>
    /// <param name="numberOfFolds">The number of folds for crossvalidation</param>
    /// <param name="parameterRanges">The ranges for each parameter in the grid search</param>
    /// <param name="seed">The random seed (required by the random forest model)</param>
    /// <param name="maxDegreeOfParallelism">The maximum allowed number of threads (to parallelize the grid search)</param>
    /// <returns>The best parameter values found by the grid search</returns>
    public static RFParameter GridSearch(IRegressionProblemData problemData, int numberOfFolds, Dictionary<string, IEnumerable<double>> parameterRanges, int seed = 12345, int maxDegreeOfParallelism = 1) {
      DoubleValue mse = new DoubleValue(Double.MaxValue);
      RFParameter bestParameter = new RFParameter();

      var setters = parameterRanges.Keys.Select(GenerateSetter).ToList();
      var partitions = GenerateRandomForestPartitions(problemData, numberOfFolds);
      var crossProduct = parameterRanges.Values.CartesianProduct();

      var locker = new object();
      Parallel.ForEach(crossProduct, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, parameterCombination => {
        var parameterValues = parameterCombination.ToList();
        double testMSE;
        var parameters = new RFParameter();
        for (int i = 0; i < setters.Count; ++i) {
          setters[i](parameters, parameterValues[i]);
        }
        CrossValidate(problemData, partitions, parameters.N, parameters.R, parameters.M, seed, out testMSE);

        lock (locker) {
          if (testMSE < mse.Value) {
            mse.Value = testMSE;
            bestParameter = (RFParameter)parameters.Clone();
          }
        }
      });
      return bestParameter;
    }

    /// <summary>
    /// Grid search with crossvalidation
    /// </summary>
    /// <param name="problemData">The classification problem data</param>
    /// <param name="numberOfFolds">The number of folds for crossvalidation</param>
    /// <param name="shuffleFolds">Specifies whether the folds should be shuffled</param>
    /// <param name="parameterRanges">The ranges for each parameter in the grid search</param>
    /// <param name="seed">The random seed (for shuffling)</param>
    /// <param name="maxDegreeOfParallelism">The maximum allowed number of threads (to parallelize the grid search)</param>
    public static RFParameter GridSearch(IClassificationProblemData problemData, int numberOfFolds, bool shuffleFolds, Dictionary<string, IEnumerable<double>> parameterRanges, int seed = 12345, int maxDegreeOfParallelism = 1) {
      DoubleValue accuracy = new DoubleValue(0);
      RFParameter bestParameter = new RFParameter();

      var setters = parameterRanges.Keys.Select(GenerateSetter).ToList();
      var crossProduct = parameterRanges.Values.CartesianProduct();
      var partitions = GenerateRandomForestPartitions(problemData, numberOfFolds, shuffleFolds);

      var locker = new object();
      Parallel.ForEach(crossProduct, new ParallelOptions { MaxDegreeOfParallelism = maxDegreeOfParallelism }, parameterCombination => {
        var parameterValues = parameterCombination.ToList();
        double testAccuracy;
        var parameters = new RFParameter();
        for (int i = 0; i < setters.Count; ++i) {
          setters[i](parameters, parameterValues[i]);
        }
        CrossValidate(problemData, partitions, parameters.N, parameters.R, parameters.M, seed, out testAccuracy);

        lock (locker) {
          if (testAccuracy > accuracy.Value) {
            accuracy.Value = testAccuracy;
            bestParameter = (RFParameter)parameters.Clone();
          }
        }
      });
      return bestParameter;
    }

    private static Tuple<IEnumerable<int>, IEnumerable<int>>[] GenerateRandomForestPartitions(IDataAnalysisProblemData problemData, int numberOfFolds, bool shuffleFolds = false) {
      var folds = GenerateFolds(problemData, numberOfFolds, shuffleFolds).ToList();
      var partitions = new Tuple<IEnumerable<int>, IEnumerable<int>>[numberOfFolds];

      for (int i = 0; i < numberOfFolds; ++i) {
        int p = i; // avoid "access to modified closure" warning
        var trainingRows = folds.SelectMany((par, j) => j != p ? par : Enumerable.Empty<int>());
        var testRows = folds[i];
        partitions[i] = new Tuple<IEnumerable<int>, IEnumerable<int>>(trainingRows, testRows);
      }
      return partitions;
    }

    public static IEnumerable<IEnumerable<int>> GenerateFolds(IDataAnalysisProblemData problemData, int numberOfFolds, bool shuffleFolds = false) {
      var random = new MersenneTwister((uint)Environment.TickCount);
      if (problemData is IRegressionProblemData) {
        var trainingIndices = shuffleFolds ? problemData.TrainingIndices.OrderBy(x => random.Next()) : problemData.TrainingIndices;
        return GenerateFolds(trainingIndices, problemData.TrainingPartition.Size, numberOfFolds);
      }
      if (problemData is IClassificationProblemData) {
        // when shuffle is enabled do stratified folds generation, some folds may have zero elements 
        // otherwise, generate folds normally
        return shuffleFolds ? GenerateFoldsStratified(problemData as IClassificationProblemData, numberOfFolds, random) : GenerateFolds(problemData.TrainingIndices, problemData.TrainingPartition.Size, numberOfFolds);
      }
      throw new ArgumentException("Problem data is neither regression or classification problem data.");
    }

    /// <summary>
    /// Stratified fold generation from classification data. Stratification means that we ensure the same distribution of class labels for each fold.
    /// The samples are grouped by class label and each group is split into @numberOfFolds parts. The final folds are formed from the joining of 
    /// the corresponding parts from each class label.
    /// </summary>
    /// <param name="problemData">The classification problem data.</param>
    /// <param name="numberOfFolds">The number of folds in which to split the data.</param>
    /// <param name="random">The random generator used to shuffle the folds.</param>
    /// <returns>An enumerable sequece of folds, where a fold is represented by a sequence of row indices.</returns>
    private static IEnumerable<IEnumerable<int>> GenerateFoldsStratified(IClassificationProblemData problemData, int numberOfFolds, IRandom random) {
      var values = problemData.Dataset.GetDoubleValues(problemData.TargetVariable, problemData.TrainingIndices);
      var valuesIndices = problemData.TrainingIndices.Zip(values, (i, v) => new { Index = i, Value = v }).ToList();
      IEnumerable<IEnumerable<IEnumerable<int>>> foldsByClass = valuesIndices.GroupBy(x => x.Value, x => x.Index).Select(g => GenerateFolds(g, g.Count(), numberOfFolds));
      var enumerators = foldsByClass.Select(f => f.GetEnumerator()).ToList();
      while (enumerators.All(e => e.MoveNext())) {
        yield return enumerators.SelectMany(e => e.Current).OrderBy(x => random.Next()).ToList();
      }
    }

    private static IEnumerable<IEnumerable<T>> GenerateFolds<T>(IEnumerable<T> values, int valuesCount, int numberOfFolds) {
      // if number of folds is greater than the number of values, some empty folds will be returned
      if (valuesCount < numberOfFolds) {
        for (int i = 0; i < numberOfFolds; ++i)
          yield return i < valuesCount ? values.Skip(i).Take(1) : Enumerable.Empty<T>();
      } else {
        int f = valuesCount / numberOfFolds, r = valuesCount % numberOfFolds; // number of folds rounded to integer and remainder
        int start = 0, end = f;
        for (int i = 0; i < numberOfFolds; ++i) {
          if (r > 0) {
            ++end;
            --r;
          }
          yield return values.Skip(start).Take(end - start);
          start = end;
          end += f;
        }
      }
    }

    private static Action<RFParameter, double> GenerateSetter(string field) {
      var targetExp = Expression.Parameter(typeof(RFParameter));
      var valueExp = Expression.Parameter(typeof(double));
      var fieldExp = Expression.Property(targetExp, field);
      var assignExp = Expression.Assign(fieldExp, Expression.Convert(valueExp, fieldExp.Type));
      var setter = Expression.Lambda<Action<RFParameter, double>>(assignExp, targetExp, valueExp).Compile();
      return setter;
    }

    private static string GetTargetVariableName(IDataAnalysisProblemData problemData) {
      var regressionProblemData = problemData as IRegressionProblemData;
      var classificationProblemData = problemData as IClassificationProblemData;

      if (regressionProblemData != null)
        return regressionProblemData.TargetVariable;
      if (classificationProblemData != null)
        return classificationProblemData.TargetVariable;

      throw new ArgumentException("Problem data is neither regression or classification problem data.");
    }
  }
}
