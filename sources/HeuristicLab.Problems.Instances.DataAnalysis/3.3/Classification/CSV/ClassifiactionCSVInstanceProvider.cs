﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2012 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using HeuristicLab.Common;
using HeuristicLab.Problems.DataAnalysis;

namespace HeuristicLab.Problems.Instances.DataAnalysis {
  public class ClassificationCSVInstanceProvider : ClassificationInstanceProvider {
    public override string Name {
      get { return "CSV File"; }
    }
    public override string Description {
      get {
        return "";
      }
    }
    public override Uri WebLink {
      get { return new Uri("http://dev.heuristiclab.com/trac/hl/core/wiki/UsersFAQ#DataAnalysisImportFileFormat"); }
    }
    public override string ReferencePublication {
      get { return ""; }
    }

    public override IEnumerable<IDataDescriptor> GetDataDescriptors() {
      return new List<IDataDescriptor>();
    }

    public override IClassificationProblemData LoadData(IDataDescriptor descriptor) {
      throw new NotImplementedException();
    }

    public override bool CanImportData {
      get { return true; }
    }
    public override IClassificationProblemData ImportData(string path) {
      TableFileParser csvFileParser = new TableFileParser();

      csvFileParser.Parse(path);

      Dataset dataset = new Dataset(csvFileParser.VariableNames, csvFileParser.Values);
      string targetVar = dataset.DoubleVariables.Last();

      // turn of input variables that are constant in the training partition
      var allowedInputVars = new List<string>();
      var trainingIndizes = Enumerable.Range(0, (csvFileParser.Rows * 2) / 3);
      if (trainingIndizes.Count() >= 2) {
        foreach (var variableName in dataset.DoubleVariables) {
          if (dataset.GetDoubleValues(variableName, trainingIndizes).Range() > 0 &&
            variableName != targetVar)
            allowedInputVars.Add(variableName);
        }
      } else {
        allowedInputVars.AddRange(dataset.DoubleVariables.Where(x => x.Equals(targetVar)));
      }

      ClassificationProblemData classificationData = new ClassificationProblemData(dataset, allowedInputVars, targetVar);

      int trainingPartEnd = trainingIndizes.Last();
      classificationData.TrainingPartition.Start = trainingIndizes.First();
      classificationData.TrainingPartition.End = trainingPartEnd;
      classificationData.TestPartition.Start = trainingPartEnd;
      classificationData.TestPartition.End = csvFileParser.Rows;

      classificationData.Name = Path.GetFileName(path);

      return classificationData;
    }

    public override IClassificationProblemData ImportData(string path, DataAnalysisImportType type) {
      TableFileParser csvFileParser = new TableFileParser();
      csvFileParser.Parse(path);

      int trainingPartEnd = (csvFileParser.Rows * type.Training) / 100;
      List<IList> values = csvFileParser.Values;
      if (type.Shuffle) {
        values = Shuffle(values);
      }

      Dataset dataset = new Dataset(csvFileParser.VariableNames, values);
      string targetVar = dataset.DoubleVariables.Last();

      // turn of input variables that are constant in the training partition
      var allowedInputVars = new List<string>();
      var trainingIndizes = Enumerable.Range(0, trainingPartEnd);
      foreach (var variableName in dataset.DoubleVariables) {
        if (trainingIndizes.Count() >= 2 && dataset.GetDoubleValues(variableName, trainingIndizes).Range() > 0 &&
          variableName != targetVar)
          allowedInputVars.Add(variableName);
      }

      ClassificationProblemData classificationData = new ClassificationProblemData(dataset, allowedInputVars, targetVar);

      classificationData.TrainingPartition.Start = 0;
      classificationData.TrainingPartition.End = trainingPartEnd;
      classificationData.TestPartition.Start = trainingPartEnd;
      classificationData.TestPartition.End = csvFileParser.Rows;

      classificationData.Name = Path.GetFileName(path);

      return classificationData;
    }

    public override bool CanExportData {
      get { return true; }
    }
    public override void ExportData(IClassificationProblemData instance, string path) {
      var strBuilder = new StringBuilder();
      var colSep = CultureInfo.CurrentCulture.TextInfo.ListSeparator;
      foreach (var variable in instance.Dataset.VariableNames) {
        strBuilder.Append(variable.Replace(colSep, String.Empty) + colSep);
      }
      strBuilder.Remove(strBuilder.Length - colSep.Length, colSep.Length);
      strBuilder.AppendLine();

      var dataset = instance.Dataset;

      for (int i = 0; i < dataset.Rows; i++) {
        for (int j = 0; j < dataset.Columns; j++) {
          if (j > 0) strBuilder.Append(colSep);
          strBuilder.Append(dataset.GetValue(i, j));
        }
        strBuilder.AppendLine();
      }

      using (var writer = new StreamWriter(path)) {
        writer.Write(strBuilder);
      }
    }
  }
}
