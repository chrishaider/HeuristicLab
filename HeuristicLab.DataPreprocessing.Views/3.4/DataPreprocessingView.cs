﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2016 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;
using HeuristicLab.Problems.DataAnalysis;
using HeuristicLab.Problems.Instances.DataAnalysis;
using HeuristicLab.Problems.Instances.DataAnalysis.Views;

namespace HeuristicLab.DataPreprocessing.Views {
  [View("DataPreprocessing View")]
  [Content(typeof(PreprocessingContext), true)]
  public partial class DataPreprocessingView : NamedItemView {

    public DataPreprocessingView() {
      InitializeComponent();
    }

    public new PreprocessingContext Content {
      get { return (PreprocessingContext)base.Content; }
      set { base.Content = value; }
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content != null) {
        var data = Content.Data;
        var filterLogic = new FilterLogic(data);
        var searchLogic = new SearchLogic(data, filterLogic);
        var statisticsLogic = new StatisticsLogic(data, searchLogic);
        var manipulationLogic = new ManipulationLogic(data, searchLogic, statisticsLogic);

        var viewShortcuts = new ItemList<IViewShortcut> {
          new DataGridContent(data, manipulationLogic, filterLogic),
          new StatisticsContent(statisticsLogic),

          new LineChartContent(data),
          new HistogramContent(data),
          new ScatterPlotContent(data),
          new CorrelationMatrixContent(Content),
          new DataCompletenessChartContent(searchLogic),

          new FilterContent(filterLogic),
          new ManipulationContent(manipulationLogic, searchLogic, filterLogic),
          new TransformationContent(data, filterLogic)
        };

        viewShortcutListView.Content = viewShortcuts.AsReadOnly();
        viewShortcutListView.ItemsListView.Items[0].Selected = true;
        viewShortcutListView.Select();

        applyTypeContextMenuStrip.Items.Clear();
        exportTypeContextMenuStrip.Items.Clear();
        foreach (var exportOption in Content.GetSourceExportOptions()) {
          var applyMenuItem = new ToolStripMenuItem(exportOption.Key) { Tag = exportOption.Value };
          applyMenuItem.Click += applyToolStripMenuItem_Click;
          applyTypeContextMenuStrip.Items.Add(applyMenuItem);

          var exportMenuItem = new ToolStripMenuItem(exportOption.Key) { Tag = exportOption.Value };
          exportMenuItem.Click += exportToolStripMenuItem_Click;
          exportTypeContextMenuStrip.Items.Add(exportMenuItem);
        }
        var exportCsvMenuItem = new ToolStripMenuItem(".csv");
        exportCsvMenuItem.Click += exportCsvMenuItem_Click;
        exportTypeContextMenuStrip.Items.Add(exportCsvMenuItem);
      } else {
        viewShortcutListView.Content = null;
      }
    }
    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.Reset += Content_Reset;
      Content.Data.FilterChanged += Data_FilterChanged;
    }

    protected override void DeregisterContentEvents() {
      base.DeregisterContentEvents();
      Content.Reset -= Content_Reset;
      Content.Data.FilterChanged -= Data_FilterChanged;
    }

    void Content_Reset(object sender, EventArgs e) {
      OnContentChanged(); // Reset by setting new content
    }

    void Data_FilterChanged(object sender, EventArgs e) {
      lblFilterActive.Visible = Content.Data.IsFiltered;
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      viewShortcutListView.Enabled = Content != null;
      applyInNewTabButton.Enabled = Content != null;
      exportProblemButton.Enabled = Content != null && Content.CanExport;
      undoButton.Enabled = Content != null;
    }

    #region New
    private void newButton_Click(object sender, EventArgs e) {
      newProblemDataTypeContextMenuStrip.Show(Cursor.Position);
    }
    private void newRegressionToolStripMenuItem_Click(object sender, EventArgs e) {
      Content.Import(new RegressionProblemData());
    }
    private void newClassificationToolStripMenuItem_Click(object sender, EventArgs e) {
      Content.Import(new ClassificationProblemData());
    }
    private void newTimeSeriesToolStripMenuItem_Click(object sender, EventArgs e) {
      Content.Import(new TimeSeriesPrognosisProblemData());
    }
    #endregion

    #region Import
    private void importButton_Click(object sender, EventArgs e) {
      importProblemDataTypeContextMenuStrip.Show(Cursor.Position);
    }
    private void importRegressionToolStripMenuItem_Click(object sender, EventArgs e) {
      Import(new RegressionCSVInstanceProvider(), new RegressionImportTypeDialog(),
        (dialog => ((RegressionImportTypeDialog)dialog).ImportType));
    }
    private void importClassificationToolStripMenuItem_Click(object sender, EventArgs e) {
      Import(new ClassificationCSVInstanceProvider(), new ClassificationImportTypeDialog(),
        (dialog => ((ClassificationImportTypeDialog)dialog).ImportType));
    }
    private void importTimeSeriesToolStripMenuItem_Click(object sender, EventArgs e) {
      Import(new TimeSeriesPrognosisCSVInstanceProvider(), new TimeSeriesPrognosisImportTypeDialog(),
        (dialog => ((TimeSeriesPrognosisImportTypeDialog)dialog).ImportType));
    }
    private void Import<TProblemData, TImportType>(DataAnalysisInstanceProvider<TProblemData, TImportType> instanceProvider, DataAnalysisImportTypeDialog importTypeDialog,
      Func<DataAnalysisImportTypeDialog, TImportType> getImportType)
      where TProblemData : class, IDataAnalysisProblemData
      where TImportType : DataAnalysisImportType {
      if (importTypeDialog.ShowDialog() == DialogResult.OK) {
        Task.Run(() => {
          TProblemData instance;
          var mainForm = (MainForm.WindowsForms.MainForm)MainFormManager.MainForm;
          // lock active view and show progress bar
          var activeView = (IContentView)MainFormManager.MainForm.ActiveView;

          try {
            var progress = mainForm.AddOperationProgressToContent(activeView.Content, "Loading problem instance.");

            instanceProvider.ProgressChanged += (o, args) => { progress.ProgressValue = args.ProgressPercentage / 100.0; };

            instance = instanceProvider.ImportData(importTypeDialog.Path, getImportType(importTypeDialog), importTypeDialog.CSVFormat);
          } catch (IOException ex) {
            MessageBox.Show(string.Format("There was an error parsing the file: {0}", Environment.NewLine + ex.Message), "Error while parsing", MessageBoxButtons.OK, MessageBoxIcon.Error);
            mainForm.RemoveOperationProgressFromContent(activeView.Content);
            return;
          }
          try {
            Content.Import(instance);
          } catch (IOException ex) {
            MessageBox.Show(string.Format("This problem does not support loading the instance {0}: {1}", Path.GetFileName(importTypeDialog.Path), Environment.NewLine + ex.Message), "Cannot load instance");
          } finally {
            mainForm.RemoveOperationProgressFromContent(activeView.Content);
          }
        });
      }
    }
    #endregion

    #region Apply
    private void applyInNewTabButton_Click(object sender, EventArgs e) {
      applyTypeContextMenuStrip.Show(Cursor.Position);
    }
    private void applyToolStripMenuItem_Click(object sender, EventArgs e) {
      var menuItem = (ToolStripMenuItem)sender;
      var itemCreator = (Func<IItem>)menuItem.Tag;
      MainFormManager.MainForm.ShowContent(itemCreator());
    }
    #endregion

    #region Export
    private void exportProblemButton_Click(object sender, EventArgs e) {
      exportTypeContextMenuStrip.Show(Cursor.Position);
    }
    private void exportToolStripMenuItem_Click(object sender, EventArgs e) {
      var menuItem = (ToolStripMenuItem)sender;
      var itemCreator = (Func<IItem>)menuItem.Tag;
      var saveFileDialog = new SaveFileDialog {
        Title = "Save Item",
        DefaultExt = "hl",
        Filter = "Uncompressed HeuristicLab Files|*.hl|HeuristicLab Files|*.hl|All Files|*.*",
        FilterIndex = 2
      };

      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        Task.Run(() => {
          bool compressed = saveFileDialog.FilterIndex != 1;
          var storable = itemCreator() as IStorableContent;
          if (storable != null) {
            var mainForm = (MainForm.WindowsForms.MainForm)MainFormManager.MainForm;
            var activeView = (IContentView)MainFormManager.MainForm.ActiveView;
            try {
              mainForm.AddOperationProgressToContent(activeView.Content, "Exporting data.");
              ContentManager.Save(storable, saveFileDialog.FileName, compressed);
            } finally {
              mainForm.RemoveOperationProgressFromContent(activeView.Content);
            }
          }
        });
      }
    }
    private void exportCsvMenuItem_Click(object sender, EventArgs e) {
      var saveFileDialog = new SaveFileDialog {
        Title = "Save Data",
        DefaultExt = "csv",
        Filter = "CSV files|*.csv|All files|*.*",
        FilterIndex = 1
      };

      if (saveFileDialog.ShowDialog() == DialogResult.OK) {
        Task.Run(() => {
          var mainForm = (MainForm.WindowsForms.MainForm)MainFormManager.MainForm;
          var activeView = (IContentView)MainFormManager.MainForm.ActiveView;
          try {
            var problemData = Content.CreateNewProblemData();
            mainForm.AddOperationProgressToContent(activeView.Content, "Exporting data.");
            if (problemData is TimeSeriesPrognosisProblemData)
              Export(new TimeSeriesPrognosisCSVInstanceProvider(), problemData, saveFileDialog.FileName);
            else if (problemData is RegressionProblemData)
              Export(new RegressionCSVInstanceProvider(), problemData, saveFileDialog.FileName);
            else if (problemData is ClassificationProblemData)
              Export(new ClassificationCSVInstanceProvider(), problemData, saveFileDialog.FileName);
          } finally {
            mainForm.RemoveOperationProgressFromContent(activeView.Content);
          }
        });
      }
    }
    private void Export<TProblemData, TImportType>(DataAnalysisInstanceProvider<TProblemData, TImportType> instanceProvider,
      IDataAnalysisProblemData problemData, string path)
      where TProblemData : class, IDataAnalysisProblemData where TImportType : DataAnalysisImportType {
      instanceProvider.ExportData((TProblemData)problemData, path);
    }
    #endregion

    #region Undo / Redo
    private void undoButton_Click(object sender, EventArgs e) {
      Content.Data.Undo();
    }
    #endregion
  }
}