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

using HeuristicLab.MainForm;
using HeuristicLab.Problems.TravelingSalesman.Views;

namespace HeuristicLab.Problems.PTSP.Views {
  [View("Euclidean TSP Data View")]
  [Content(typeof(EuclideanPTSPData), IsDefaultView = true)]
  public partial class EuclideanPTSPDataView : EuclideanTSPDataView {

    public new EuclideanPTSPData Content {
      get { return (EuclideanPTSPData)base.Content; }
      set { base.Content = value; }
    }

    public EuclideanPTSPDataView() {
      InitializeComponent();
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content == null) {

      } else {

      }
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();

    }
  }
}
