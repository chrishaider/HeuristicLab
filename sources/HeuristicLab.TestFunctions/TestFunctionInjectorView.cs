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
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

using HeuristicLab.Core;
using HeuristicLab.Data;

namespace HeuristicLab.TestFunctions {
  public partial class TestFunctionInjectorView : ViewBase {
    public TestFunctionInjector TestFunctionInjector {
      get { return (TestFunctionInjector)base.Item; }
      set { base.Item = value; }
    }

    public TestFunctionInjectorView() {
      InitializeComponent();
    }

    public TestFunctionInjectorView(TestFunctionInjector testFunctionInjector)
      : this() {
      TestFunctionInjector = testFunctionInjector;
    }

    protected override void RemoveItemEvents() {
      maximizationCheckBox.DataBindings.Clear();
      dimensionTextBox.DataBindings.Clear();
      lowerBoundTextBox.DataBindings.Clear();
      upperBoundTextBox.DataBindings.Clear();
      base.RemoveItemEvents();
    }

    protected override void AddItemEvents() {
      base.AddItemEvents();
      maximizationCheckBox.DataBindings.Add("Checked", TestFunctionInjector, "Maximization");
      dimensionTextBox.DataBindings.Add("Text", TestFunctionInjector, "Dimension");
      lowerBoundTextBox.DataBindings.Add("Text", TestFunctionInjector, "LowerBound");
      upperBoundTextBox.DataBindings.Add("Text", TestFunctionInjector, "UpperBound");
    }

    protected override void UpdateControls() {
      base.UpdateControls();
      if (TestFunctionInjector == null) {
        maximizationCheckBox.Enabled = false;
        maximizationCheckBox.Checked = false;
        dimensionTextBox.Enabled = false;
        dimensionTextBox.Text = "-";
        lowerBoundTextBox.Enabled = false;
        lowerBoundTextBox.Text = "-";
        upperBoundTextBox.Enabled = false;
        upperBoundTextBox.Text = "-";
      } else {
        maximizationCheckBox.Enabled = true;
        dimensionTextBox.Enabled = true;
        lowerBoundTextBox.Enabled = true;
        upperBoundTextBox.Enabled = true;
      }
    }
  }
}
