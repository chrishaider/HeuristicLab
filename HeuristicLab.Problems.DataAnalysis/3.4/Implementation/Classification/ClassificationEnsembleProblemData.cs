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
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Parameters;
using HEAL.Attic;

namespace HeuristicLab.Problems.DataAnalysis {
  [StorableType("58768587-0920-4B52-95E4-66B54E8E837C")]
  [Item("ClassificationEnsembleProblemData", "Represents an item containing all data defining a classification problem.")]
  public class ClassificationEnsembleProblemData : ClassificationProblemData {

    public override bool IsTrainingSample(int index) {
      return index >= 0 && index < Dataset.Rows &&
             TrainingPartition.Start <= index && index < TrainingPartition.End;
    }

    public override bool IsTestSample(int index) {
      return index >= 0 && index < Dataset.Rows &&
             TestPartition.Start <= index && index < TestPartition.End;
    }

    private static readonly ClassificationEnsembleProblemData emptyProblemData;
    public static new ClassificationEnsembleProblemData EmptyProblemData {
      get { return emptyProblemData; }
    }

    static ClassificationEnsembleProblemData() {
      var problemData = new ClassificationEnsembleProblemData();
      problemData.Parameters.Clear();
      problemData.Name = "Empty Classification ProblemData";
      problemData.Description = "This ProblemData acts as place holder before the correct problem data is loaded.";
      problemData.isEmpty = true;

      problemData.Parameters.Add(new FixedValueParameter<Dataset>(DatasetParameterName, "", new Dataset()));
      problemData.Parameters.Add(new FixedValueParameter<ReadOnlyCheckedItemList<StringValue>>(InputVariablesParameterName, ""));
      problemData.Parameters.Add(new FixedValueParameter<IntRange>(TrainingPartitionParameterName, "", (IntRange)new IntRange(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<IntRange>(TestPartitionParameterName, "", (IntRange)new IntRange(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new ConstrainedValueParameter<StringValue>(TargetVariableParameterName, new ItemSet<StringValue>()));
      problemData.Parameters.Add(new FixedValueParameter<StringMatrix>(ClassNamesParameterName, "", new StringMatrix(0, 0).AsReadOnly()));
      problemData.Parameters.Add(new FixedValueParameter<DoubleMatrix>(ClassificationPenaltiesParameterName, "", (DoubleMatrix)new DoubleMatrix(0, 0).AsReadOnly()));
      emptyProblemData = problemData;
    }

    [StorableConstructor]
    protected ClassificationEnsembleProblemData(StorableConstructorFlag _) : base(_) { }
    protected ClassificationEnsembleProblemData(ClassificationEnsembleProblemData original, Cloner cloner) : base(original, cloner) { }
    public override IDeepCloneable Clone(Cloner cloner) {
      if (this == emptyProblemData) return emptyProblemData;
      return new ClassificationEnsembleProblemData(this, cloner);
    }

    public ClassificationEnsembleProblemData() : base() { }

    public ClassificationEnsembleProblemData(IClassificationProblemData classificationProblemData)
      : base(classificationProblemData) {
    }

    public ClassificationEnsembleProblemData(Dataset dataset, IEnumerable<string> allowedInputVariables, string targetVariable)
      : base(dataset, allowedInputVariables, targetVariable) {
    }

    public ClassificationEnsembleProblemData(Dataset dataset, IEnumerable<string> allowedInputVariables, string targetVariable, IEnumerable<string> classNames, string positiveClass = null)
      : base(dataset, allowedInputVariables, targetVariable, classNames, positiveClass) {
    }
  }
}
