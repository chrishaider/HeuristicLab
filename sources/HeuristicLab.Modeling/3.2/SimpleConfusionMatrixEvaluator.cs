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

using HeuristicLab.Core;
using HeuristicLab.Data;

namespace HeuristicLab.Modeling {
  public class SimpleConfusionMatrixEvaluator : OperatorBase {
    protected const int ORIGINAL_INDEX = 0;
    protected const int ESTIMATION_INDEX = 1;
    public override string Description {
      get {
        return @"Calculates the classifcation matrix of the model.";
      }
    }

    public SimpleConfusionMatrixEvaluator()
      : base() {
      AddVariableInfo(new VariableInfo("Values", "Original and predicted target values generated by a model", typeof(DoubleMatrixData), VariableKind.In));
      AddVariableInfo(new VariableInfo("ConfusionMatrix", "The confusion matrix of the model", typeof(IntMatrixData), VariableKind.New));
    }

    public override IOperation Apply(IScope scope) {
      double[,] values = GetVariableValue<DoubleMatrixData>("Values", scope, true).Data;
      int[,] confusionMatrix = Calculate(values);
      IntMatrixData matrix = GetVariableValue<IntMatrixData>("ConfusionMatrix", scope, false, false);
      if (matrix == null) {
        matrix = new IntMatrixData(confusionMatrix);
        scope.AddVariable(new HeuristicLab.Core.Variable(scope.TranslateName("ConfusionMatrix"), matrix));
      }

      return null;
    }

    public static int[,] Calculate(double[,] values) {
      double[] classes = SimpleAccuracyEvaluator.CalculateTargetClasses(values);
      double[] thresholds = SimpleAccuracyEvaluator.CalculateThresholds(classes);
      int nSamples = values.GetLength(0);
      int[,] confusionMatrix = new int[classes.Length, classes.Length];
      for (int sample = 0; sample < nSamples; sample++) {
        double est = values[sample, ESTIMATION_INDEX];
        double origClass = values[sample, ORIGINAL_INDEX];
        int estClassIndex = -1;
        // if estimation is lower than the smallest threshold value -> estimated class is the lower class
        if (est < thresholds[0]) estClassIndex = 0;
        // if estimation is larger (or equal) than the largest threshold value -> estimated class is the upper class
        else if (est >= thresholds[thresholds.Length - 1]) estClassIndex = classes.Length - 1;
        else {
          // otherwise the estimated class is the class which upper threshold is larger than the estimated value
          for (int k = 0; k < thresholds.Length; k++) {
            if (thresholds[k] > est) {
              estClassIndex = k;
              break;
            }
          }
        }

        // find the first threshold index that is larger to the original value
        int origClassIndex = classes.Length - 1;
        for (int i = 0; i < thresholds.Length; i++) {
          if (origClass < thresholds[i]) {
            origClassIndex = i;
            break;
          }
        }
        confusionMatrix[origClassIndex, estClassIndex]++;
      }
      return confusionMatrix;
    }
  }
}
