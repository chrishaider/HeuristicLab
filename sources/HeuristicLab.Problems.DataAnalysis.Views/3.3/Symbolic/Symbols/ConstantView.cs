﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2010 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.Linq;
using System.Text;
using System.Windows.Forms;
using HeuristicLab.MainForm;
using HeuristicLab.MainForm.WindowsForms;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding.Symbols;
using HeuristicLab.Core.Views;
using HeuristicLab.Problems.DataAnalysis.Symbolic.Symbols;

namespace HeuristicLab.Encodings.SymbolicExpressionTreeEncoding.Views {
  [View("Constant View")]
  [Content(typeof(Constant), true)]
  public partial class ConstantView : SymbolView {
    public new Constant Content {
      get { return (Constant)base.Content; }
      set { base.Content = value; }
    }

    public ConstantView() {
      InitializeComponent();
    }

    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.Changed += new EventHandler(Content_Changed);
    }

    protected override void DeregisterContentEvents() {
      base.DeregisterContentEvents();
      Content.Changed -= new EventHandler(Content_Changed);
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      UpdateControl();
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      minValueTextBox.Enabled = Content != null;
      maxValueTextBox.Enabled = Content != null;
      valueChangeNuTextBox.Enabled = Content != null;
      valueChangeSigmaTextBox.Enabled = Content != null;
    }

    #region content event handlers
    private void Content_Changed(object sender, EventArgs e) {
      UpdateControl();
    }
    #endregion

    #region control event handlers
    private void minValueTextBox_TextChanged(object sender, EventArgs e) {
      double min;
      if (double.TryParse(minValueTextBox.Text, out min)) {
        Content.MinValue = min;
        errorProvider.SetError(minValueTextBox, string.Empty);
      } else {
        errorProvider.SetError(minValueTextBox, "Invalid value");
      }
    }
    private void maxValueTextBox_TextChanged(object sender, EventArgs e) {
      double max;
      if (double.TryParse(maxValueTextBox.Text, out max)) {
        Content.MaxValue = max;
        errorProvider.SetError(maxValueTextBox, string.Empty);
      } else {
        errorProvider.SetError(maxValueTextBox, "Invalid value");
      }
    }

    private void valueChangeNuTextBox_TextChanged(object sender, EventArgs e) {
      double nu;
      if (double.TryParse(valueChangeNuTextBox.Text, out nu)) {
        Content.ManipulatorNu = nu;
        errorProvider.SetError(valueChangeNuTextBox, string.Empty);
      } else {
        errorProvider.SetError(valueChangeNuTextBox, "Invalid value");
      }
    }

    private void valueChangeSigmaTextBox_TextChanged(object sender, EventArgs e) {
      double sigma;
      if (double.TryParse(valueChangeSigmaTextBox.Text, out sigma) && sigma >= 0.0) {
        Content.ManipulatorSigma = sigma;
        errorProvider.SetError(valueChangeSigmaTextBox, string.Empty);
      } else {
        errorProvider.SetError(valueChangeSigmaTextBox, "Invalid value");
      }
    }
    #endregion

    #region helpers
    private void UpdateControl() {
      if (Content == null) {
        minValueTextBox.Text = string.Empty;
        maxValueTextBox.Text = string.Empty;
        minValueTextBox.Text = string.Empty;
        valueChangeSigmaTextBox.Text = string.Empty;
      } else {
        minValueTextBox.Text = Content.MinValue.ToString();
        maxValueTextBox.Text = Content.MaxValue.ToString();
        valueChangeNuTextBox.Text = Content.ManipulatorNu.ToString();
        valueChangeSigmaTextBox.Text = Content.ManipulatorSigma.ToString();
      }
      SetEnabledStateOfControls();
    }
    #endregion
  }
}
