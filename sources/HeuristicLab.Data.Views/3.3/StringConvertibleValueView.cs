#region License Information
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
using System.ComponentModel;
using System.Windows.Forms;
using HeuristicLab.MainForm;
using HeuristicLab.MainForm.WindowsForms;

namespace HeuristicLab.Data.Views {
  [View("StringConvertibleValue View")]
  [Content(typeof(IStringConvertibleValue), true)]
  public partial class StringConvertibleValueView : AsynchronousContentView {
    public new IStringConvertibleValue Content {
      get { return (IStringConvertibleValue)base.Content; }
      set { base.Content = value; }
    }

    public StringConvertibleValueView() {
      InitializeComponent();
      Caption = "StringConvertibleValue View";
      errorProvider.SetIconAlignment(valueTextBox, ErrorIconAlignment.MiddleLeft);
      errorProvider.SetIconPadding(valueTextBox, 2);
    }
    public StringConvertibleValueView(IStringConvertibleValue content)
      : this() {
      Content = content;
    }

    protected override void DeregisterContentEvents() {
      Content.ValueChanged -= new EventHandler(Content_ValueChanged);
      base.DeregisterContentEvents();
    }

    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.ValueChanged += new EventHandler(Content_ValueChanged);
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content == null) {
        Caption = "StringConvertibleValue View";
        valueTextBox.Text = string.Empty;
      } else {
        Caption = Content.GetValue() + " (" + Content.GetType().Name + ")";
        valueTextBox.Text = Content.GetValue();
      }
      SetEnabledStateOfControls();
    }
    protected override void OnReadOnlyChanged() {
      base.OnReadOnlyChanged();
      SetEnabledStateOfControls();
    }
    private void SetEnabledStateOfControls() {
      if (Content == null) valueTextBox.Enabled = false;
      else {
        valueTextBox.Enabled = true;
        valueTextBox.ReadOnly = ReadOnly;
      }
    }

    private void Content_ValueChanged(object sender, EventArgs e) {
      if (InvokeRequired)
        Invoke(new EventHandler(Content_ValueChanged), sender, e);
      else
        valueTextBox.Text = Content.GetValue();
    }

    private void valueTextBox_KeyDown(object sender, KeyEventArgs e) {
      if ((e.KeyCode == Keys.Enter) || (e.KeyCode == Keys.Return))
        valueLabel.Focus();  // set focus on label to validate data
      if (e.KeyCode == Keys.Escape) {
        valueTextBox.Text = Content.GetValue();
        valueLabel.Focus();  // set focus on label to validate data
      }
    }
    private void valueTextBox_Validating(object sender, CancelEventArgs e) {
      string errorMessage;
      if (!Content.Validate(valueTextBox.Text, out errorMessage)) {
        e.Cancel = true;
        errorProvider.SetError(valueTextBox, errorMessage);
        valueTextBox.SelectAll();
      }
    }
    private void valueTextBox_Validated(object sender, EventArgs e) {
      Content.SetValue(valueTextBox.Text);
      errorProvider.SetError(valueTextBox, string.Empty);
    }
  }
}
