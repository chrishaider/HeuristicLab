﻿#region License Information
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
using System.Linq;
using System.Text;
using HeuristicLab.Core;
using System.Xml;
using System.Diagnostics;
using HeuristicLab.DataAnalysis;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.GP.StructureIdentification;
using HeuristicLab.Modeling;
using HeuristicLab.GP;
using HeuristicLab.Random;
using HeuristicLab.GP.Interfaces;
using HeuristicLab.GP.StructureIdentification.Classification;

namespace HeuristicLab.LinearRegression {
  public class LinearClassification : LinearRegression {

    public override string Name { get { return "LinearClassification"; } }

    public LinearClassification()
      : base() {
    }

    protected override IOperator CreateModelAnalyser() {
      return DefaultClassificationAlgorithmOperators.CreatePostProcessingOperator();
    }

    protected internal virtual IAnalyzerModel CreateLRModel(IScope bestModelScope) {
      IAnalyzerModel model = base.CreateLRModel(bestModelScope);
      DefaultClassificationAlgorithmOperators.SetModelData(model, bestModelScope);
      return model;
    }
  }
}
