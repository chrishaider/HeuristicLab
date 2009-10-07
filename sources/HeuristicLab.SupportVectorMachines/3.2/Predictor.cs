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

using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using HeuristicLab.Core;
using System.Globalization;
using System.IO;
using HeuristicLab.Modeling;
using SVM;
using HeuristicLab.DataAnalysis;

namespace HeuristicLab.SupportVectorMachines {
  public class Predictor : PredictorBase {
    private SVMModel svmModel;
    public SVMModel Model {
      get { return svmModel; }
    }

    private Dictionary<string, int> variableNames = new Dictionary<string, int>();
    private string targetVariable;
    private int minTimeOffset;
    public int MinTimeOffset {
      get { return minTimeOffset; }
    }
    private int maxTimeOffset;
    public int MaxTimeOffset {
      get { return maxTimeOffset; }
    }

    public Predictor() : base() { } // for persistence

    public Predictor(SVMModel model, string targetVariable, Dictionary<string, int> variableNames) :
      this(model, targetVariable, variableNames, 0, 0) {
    }

    public Predictor(SVMModel model, string targetVariable, Dictionary<string, int> variableNames, int minTimeOffset, int maxTimeOffset)
      : base() {
      this.svmModel = model;
      this.targetVariable = targetVariable;
      this.variableNames = variableNames;
      this.minTimeOffset = minTimeOffset;
      this.maxTimeOffset = maxTimeOffset;
    }

    public override double[] Predict(Dataset input, int start, int end) {
      if (start < 0 || end <= start) throw new ArgumentException("start must be larger than zero and strictly smaller than end");
      if (end > input.Rows) throw new ArgumentOutOfRangeException("number of rows in input is smaller then end");
      RangeTransform transform = svmModel.RangeTransform;
      Model model = svmModel.Model;
      // maps columns of the current input dataset to the columns that were originally used in training
      Dictionary<int, int> newIndex = new Dictionary<int, int>();
      foreach (var pair in variableNames) {
        newIndex[input.GetVariableIndex(pair.Key)] = pair.Value;
      }


      Problem p = SVMHelper.CreateSVMProblem(input, input.GetVariableIndex(targetVariable), newIndex,
        start, end, minTimeOffset, maxTimeOffset);
      Problem scaledProblem = transform.Scale(p);

      int targetVariableIndex = input.GetVariableIndex(targetVariable);
      int rows = end - start;
      double[] result = new double[rows];
      int problemRow = 0;
      for (int resultRow = 0; resultRow < rows; resultRow++) {
        if (double.IsNaN(input.GetValue(resultRow, targetVariableIndex)))
          result[resultRow] = UpperPredictionLimit;
        else
          result[resultRow] = Math.Max(Math.Min(SVM.Prediction.Predict(model, scaledProblem.X[problemRow++]), UpperPredictionLimit), LowerPredictionLimit);
      }
      return result;
    }

    public override IEnumerable<string> GetInputVariables() {
      return from pair in variableNames
             where pair.Key != targetVariable
             orderby pair.Value
             select pair.Key;
    }

    public override IView CreateView() {
      return new PredictorView(this);
    }

    public override object Clone(IDictionary<Guid, object> clonedObjects) {
      Predictor clone = (Predictor)base.Clone(clonedObjects);
      clone.svmModel = (SVMModel)Auxiliary.Clone(svmModel, clonedObjects);
      clone.targetVariable = targetVariable;
      clone.variableNames = new Dictionary<string, int>(variableNames);
      clone.minTimeOffset = minTimeOffset;
      clone.maxTimeOffset = maxTimeOffset;
      return clone;
    }

    public override XmlNode GetXmlNode(string name, XmlDocument document, IDictionary<Guid, IStorable> persistedObjects) {
      XmlNode node = base.GetXmlNode(name, document, persistedObjects);
      XmlAttribute targetVarAttr = document.CreateAttribute("TargetVariable");
      targetVarAttr.Value = targetVariable;
      node.Attributes.Append(targetVarAttr);
      XmlAttribute minTimeOffsetAttr = document.CreateAttribute("MinTimeOffset");
      XmlAttribute maxTimeOffsetAttr = document.CreateAttribute("MaxTimeOffset");
      minTimeOffsetAttr.Value = XmlConvert.ToString(minTimeOffset);
      maxTimeOffsetAttr.Value = XmlConvert.ToString(maxTimeOffset);
      node.Attributes.Append(minTimeOffsetAttr);
      node.Attributes.Append(maxTimeOffsetAttr);
      node.AppendChild(PersistenceManager.Persist(svmModel, document, persistedObjects));
      XmlNode variablesNode = document.CreateElement("Variables");
      foreach (var pair in variableNames) {
        XmlNode pairNode = document.CreateElement("Variable");
        XmlAttribute nameAttr = document.CreateAttribute("Name");
        XmlAttribute indexAttr = document.CreateAttribute("Index");
        nameAttr.Value = pair.Key;
        indexAttr.Value = XmlConvert.ToString(pair.Value);
        pairNode.Attributes.Append(nameAttr);
        pairNode.Attributes.Append(indexAttr);
        variablesNode.AppendChild(pairNode);
      }
      node.AppendChild(variablesNode);
      return node;
    }

    public override void Populate(XmlNode node, IDictionary<Guid, IStorable> restoredObjects) {
      base.Populate(node, restoredObjects);
      targetVariable = node.Attributes["TargetVariable"].Value;
      svmModel = (SVMModel)PersistenceManager.Restore(node.ChildNodes[0], restoredObjects);

      if (node.Attributes["MinTimeOffset"] != null) minTimeOffset = XmlConvert.ToInt32(node.Attributes["MinTimeOffset"].Value);
      if (node.Attributes["MaxTimeOffset"] != null) maxTimeOffset = XmlConvert.ToInt32(node.Attributes["MaxTimeOffset"].Value);
      variableNames = new Dictionary<string, int>();
      XmlNode variablesNode = node.ChildNodes[1];
      foreach (XmlNode pairNode in variablesNode.ChildNodes) {
        variableNames[pairNode.Attributes["Name"].Value] = XmlConvert.ToInt32(pairNode.Attributes["Index"].Value);
      }
    }

    public static void Export(Predictor p, Stream s) {
      StreamWriter writer = new StreamWriter(s);
      writer.Write("Targetvariable: "); writer.WriteLine(p.targetVariable);
      writer.Write("LowerPredictionLimit: "); writer.WriteLine(p.LowerPredictionLimit.ToString());
      writer.Write("UpperPredictionLimit: "); writer.WriteLine(p.UpperPredictionLimit.ToString());
      writer.Write("MaxTimeOffset: "); writer.WriteLine(p.MaxTimeOffset.ToString());
      writer.Write("MinTimeOffset: "); writer.WriteLine(p.MinTimeOffset.ToString());
      writer.Write("InputVariables :");
      writer.Write(p.GetInputVariables().First());
      foreach (string variable in p.GetInputVariables().Skip(1)) {
        writer.Write("; "); writer.Write(variable);
      }
      writer.WriteLine();
      writer.Flush();
      using (MemoryStream memStream = new MemoryStream()) {
        SVMModel.Export(p.Model, memStream);
        memStream.WriteTo(s);
      }
    }

    public static Predictor Import(Stream s) {
      Predictor p = new Predictor();
      StreamReader reader = new StreamReader(s);
      string[] targetVariableLine = reader.ReadLine().Split(':');
      string[] lowerPredictionLimitLine = reader.ReadLine().Split(':');
      string[] upperPredictionLimitLine = reader.ReadLine().Split(':');
      string[] maxTimeOffsetLine = reader.ReadLine().Split(':');
      string[] minTimeOffsetLine = reader.ReadLine().Split(':');
      string[] inputVariableLine = reader.ReadLine().Split(':', ';');

      p.targetVariable = targetVariableLine[1].Trim();
      p.LowerPredictionLimit = double.Parse(lowerPredictionLimitLine[1]);
      p.UpperPredictionLimit = double.Parse(upperPredictionLimitLine[1]);
      p.maxTimeOffset = int.Parse(maxTimeOffsetLine[1]);
      p.minTimeOffset = int.Parse(minTimeOffsetLine[1]);
      int i = 1;
      foreach (string inputVariable in inputVariableLine.Skip(1)) {
        p.variableNames[inputVariable.Trim()] = i++;
      }
      p.svmModel = SVMModel.Import(s);
      return p;
    }
  }
}
