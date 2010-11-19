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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using HeuristicLab.Collections;
using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;

namespace HeuristicLab.Analysis.Views {
  /// <summary>
  /// The visual representation of a <see cref="Variable"/>.
  /// </summary>
  [View("DataTable View")]
  [Content(typeof(DataTable), true)]
  public sealed partial class DataTableView : NamedItemView {
    private List<Series> invisibleSeries;
    private Dictionary<IObservableList<double>, DataRow> valuesRowsTable;
    /// <summary>
    /// Gets or sets the variable to represent visually.
    /// </summary>
    /// <remarks>Uses property <see cref="ViewBase.Item"/> of base class <see cref="ViewBase"/>.
    /// No own data storage present.</remarks>
    public new DataTable Content {
      get { return (DataTable)base.Content; }
      set { base.Content = value; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="VariableView"/> with caption "Variable".
    /// </summary>
    public DataTableView() {
      InitializeComponent();
      valuesRowsTable = new Dictionary<IObservableList<double>, DataRow>();
      invisibleSeries = new List<Series>();
      chart.CustomizeAllChartAreas();
      chart.ChartAreas[0].CursorX.Interval = 1;
    }

    /// <summary>
    /// Removes the eventhandlers from the underlying <see cref="Variable"/>.
    /// </summary>
    /// <remarks>Calls <see cref="ViewBase.RemoveItemEvents"/> of base class <see cref="ViewBase"/>.</remarks>
    protected override void DeregisterContentEvents() {
      foreach (DataRow row in Content.Rows)
        DeregisterDataRowEvents(row);
      Content.Rows.ItemsAdded -= new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsAdded);
      Content.Rows.ItemsRemoved -= new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsRemoved);
      Content.Rows.ItemsReplaced -= new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsReplaced);
      Content.Rows.CollectionReset -= new CollectionItemsChangedEventHandler<DataRow>(Rows_CollectionReset);
      base.DeregisterContentEvents();
    }

    /// <summary>
    /// Adds eventhandlers to the underlying <see cref="Variable"/>.
    /// </summary>
    /// <remarks>Calls <see cref="ViewBase.AddItemEvents"/> of base class <see cref="ViewBase"/>.</remarks>
    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.Rows.ItemsAdded += new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsAdded);
      Content.Rows.ItemsRemoved += new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsRemoved);
      Content.Rows.ItemsReplaced += new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsReplaced);
      Content.Rows.CollectionReset += new CollectionItemsChangedEventHandler<DataRow>(Rows_CollectionReset);
      foreach (DataRow row in Content.Rows)
        RegisterDataRowEvents(row);
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      invisibleSeries.Clear();
      chart.Titles[0].Text = string.Empty;
      chart.Series.Clear();
      if (Content != null) {
        chart.Titles[0].Text = Content.Name;
        foreach (DataRow row in Content.Rows)
          AddDataRow(row);
      }
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      chart.Enabled = Content != null;
    }

    private void AddDataRow(DataRow row) {
      Series series = new Series(row.Name);
      switch (row.VisualProperties.ChartType) {
        case DataRowVisualProperties.DataRowChartType.Line:
          series.ChartType = SeriesChartType.FastLine;
          break;
        case DataRowVisualProperties.DataRowChartType.Bars:
          series.ChartType = SeriesChartType.Bar;
          break;
        case DataRowVisualProperties.DataRowChartType.Columns:
          series.ChartType = SeriesChartType.Column;
          break;
        case DataRowVisualProperties.DataRowChartType.Points:
          series.ChartType = SeriesChartType.FastPoint;
          break;
        default:
          series.ChartType = SeriesChartType.FastPoint;
          break;
      }
      series.YAxisType = row.VisualProperties.SecondYAxis ? AxisType.Secondary : AxisType.Primary;
      if (row.VisualProperties.Color != Color.Empty) series.Color = row.VisualProperties.Color;
      series.ToolTip = row.Name + " X = #INDEX, Y = #VAL";
      FillSeriesWithRowValues(series, row);
      chart.Series.Add(series);
      chart.ChartAreas[0].RecalculateAxesScale();
      UpdateYCursorInterval();
    }

    private void UpdateYCursorInterval() {
      double interestingValuesRange = (from series in chart.Series
                                       where series.Enabled
                                       let values = (from point in series.Points
                                                     where !point.IsEmpty
                                                     select point.YValues[0])
                                                     .DefaultIfEmpty(1.0)
                                       let range = values.Max() - values.Min()
                                       where range > 0.0
                                       select range)
                                       .DefaultIfEmpty(1.0)
                                       .Min();

      double digits = (int)Math.Log10(interestingValuesRange) - 3;
      double yZoomInterval = Math.Pow(10, digits);
      this.chart.ChartAreas[0].CursorY.Interval = yZoomInterval;
    }

    private void RemoveDataRow(DataRow row) {
      Series series = chart.Series[row.Name];
      chart.Series.Remove(series);
      if (invisibleSeries.Contains(series))
        invisibleSeries.Remove(series);
      chart.ChartAreas[0].RecalculateAxesScale();
    }

    #region Content Events
    private void RegisterDataRowEvents(DataRow row) {
      row.NameChanged += new EventHandler(Row_NameChanged);
      row.VisualPropertiesChanged += new EventHandler(Row_VisualPropertiesChanged);
      valuesRowsTable.Add(row.Values, row);
      row.Values.ItemsAdded += new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsAdded);
      row.Values.ItemsRemoved += new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsRemoved);
      row.Values.ItemsReplaced += new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsReplaced);
      row.Values.ItemsMoved += new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsMoved);
      row.Values.CollectionReset += new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_CollectionReset);
    }
    private void DeregisterDataRowEvents(DataRow row) {
      row.Values.ItemsAdded -= new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsAdded);
      row.Values.ItemsRemoved -= new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsRemoved);
      row.Values.ItemsReplaced -= new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsReplaced);
      row.Values.ItemsMoved -= new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsMoved);
      row.Values.CollectionReset -= new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_CollectionReset);
      valuesRowsTable.Remove(row.Values);
      row.VisualPropertiesChanged -= new EventHandler(Row_VisualPropertiesChanged);
      row.NameChanged -= new EventHandler(Row_NameChanged);
    }
    protected override void Content_NameChanged(object sender, EventArgs e) {
      if (InvokeRequired)
        Invoke(new EventHandler(Content_NameChanged), sender, e);
      else {
        chart.Titles[0].Text = Content.Name;
        base.Content_NameChanged(sender, e);
      }
    }
    private void Rows_ItemsAdded(object sender, CollectionItemsChangedEventArgs<DataRow> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsAdded), sender, e);
      else {
        foreach (DataRow row in e.Items) {
          AddDataRow(row);
          RegisterDataRowEvents(row);
        }
      }
    }
    private void Rows_ItemsRemoved(object sender, CollectionItemsChangedEventArgs<DataRow> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsRemoved), sender, e);
      else {
        foreach (DataRow row in e.Items) {
          DeregisterDataRowEvents(row);
          RemoveDataRow(row);
        }
      }
    }
    private void Rows_ItemsReplaced(object sender, CollectionItemsChangedEventArgs<DataRow> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<DataRow>(Rows_ItemsReplaced), sender, e);
      else {
        foreach (DataRow row in e.OldItems) {
          DeregisterDataRowEvents(row);
          RemoveDataRow(row);
        }
        foreach (DataRow row in e.Items) {
          AddDataRow(row);
          RegisterDataRowEvents(row);
        }
      }
    }
    private void Rows_CollectionReset(object sender, CollectionItemsChangedEventArgs<DataRow> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<DataRow>(Rows_CollectionReset), sender, e);
      else {
        foreach (DataRow row in e.OldItems) {
          DeregisterDataRowEvents(row);
          RemoveDataRow(row);
        }
        foreach (DataRow row in e.Items) {
          AddDataRow(row);
          RegisterDataRowEvents(row);
        }
      }
    }
    private void Row_VisualPropertiesChanged(object sender, EventArgs e) {
      if (InvokeRequired)
        Invoke(new EventHandler(Row_VisualPropertiesChanged), sender, e);
      else {
        DataRow row = (DataRow)sender;
        switch (row.VisualProperties.ChartType) {
          case DataRowVisualProperties.DataRowChartType.Line:
            chart.Series[row.Name].ChartType = SeriesChartType.FastLine;
            break;
          case DataRowVisualProperties.DataRowChartType.Bars:
            chart.Series[row.Name].ChartType = SeriesChartType.Bar;
            break;
          case DataRowVisualProperties.DataRowChartType.Columns:
            chart.Series[row.Name].ChartType = SeriesChartType.Column;
            break;
          case DataRowVisualProperties.DataRowChartType.Points:
            chart.Series[row.Name].ChartType = SeriesChartType.FastPoint;
            break;
          default:
            chart.Series[row.Name].ChartType = SeriesChartType.FastPoint;
            break;
        }
        chart.Series[row.Name].YAxisType = row.VisualProperties.SecondYAxis ? AxisType.Secondary : AxisType.Primary;
        if (row.VisualProperties.Color != Color.Empty) chart.Series[row.Name].Color = row.VisualProperties.Color;
        chart.ChartAreas[0].RecalculateAxesScale();
      }
    }
    private void Row_NameChanged(object sender, EventArgs e) {
      if (InvokeRequired)
        Invoke(new EventHandler(Row_NameChanged), sender, e);
      else {
        DataRow row = (DataRow)sender;
        chart.Series[row.Name].Name = row.Name;
      }
    }
    private void Values_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IndexedItem<double>> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsAdded), sender, e);
      else {
        DataRow row = null;
        valuesRowsTable.TryGetValue((IObservableList<double>)sender, out row);
        if (row != null) {
          Series rowSeries = chart.Series[row.Name];
          if (!invisibleSeries.Contains(rowSeries)) {
            rowSeries.Points.Clear();
            FillSeriesWithRowValues(rowSeries, row);
            UpdateYCursorInterval();
          }
        }
      }
    }
    private void Values_ItemsRemoved(object sender, CollectionItemsChangedEventArgs<IndexedItem<double>> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsRemoved), sender, e);
      else {
        DataRow row = null;
        valuesRowsTable.TryGetValue((IObservableList<double>)sender, out row);
        if (row != null) {
          Series rowSeries = chart.Series[row.Name];
          if (!invisibleSeries.Contains(rowSeries)) {
            rowSeries.Points.Clear();
            FillSeriesWithRowValues(rowSeries, row);
            UpdateYCursorInterval();
          }
        }
      }
    }
    private void Values_ItemsReplaced(object sender, CollectionItemsChangedEventArgs<IndexedItem<double>> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsReplaced), sender, e);
      else {
        DataRow row = null;
        valuesRowsTable.TryGetValue((IObservableList<double>)sender, out row);
        if (row != null) {
          Series rowSeries = chart.Series[row.Name];
          if (!invisibleSeries.Contains(rowSeries)) {
            foreach (IndexedItem<double> item in e.Items) {
              if (IsInvalidValue(item.Value))
                rowSeries.Points[item.Index].IsEmpty = true;
              else {
                rowSeries.Points[item.Index].YValues = new double[] { item.Value };
                rowSeries.Points[item.Index].IsEmpty = false;
              }
            }
            UpdateYCursorInterval();
          }
        }
      }
    }
    private void Values_ItemsMoved(object sender, CollectionItemsChangedEventArgs<IndexedItem<double>> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_ItemsMoved), sender, e);
      else {
        DataRow row = null;
        valuesRowsTable.TryGetValue((IObservableList<double>)sender, out row);
        if (row != null) {
          Series rowSeries = chart.Series[row.Name];
          if (!invisibleSeries.Contains(rowSeries)) {
            rowSeries.Points.Clear();
            FillSeriesWithRowValues(rowSeries, row);
            UpdateYCursorInterval();
          }
        }
      }
    }

    private void Values_CollectionReset(object sender, CollectionItemsChangedEventArgs<IndexedItem<double>> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<IndexedItem<double>>(Values_CollectionReset), sender, e);
      else {
        DataRow row = null;
        valuesRowsTable.TryGetValue((IObservableList<double>)sender, out row);
        if (row != null) {
          Series rowSeries = chart.Series[row.Name];
          if (!invisibleSeries.Contains(rowSeries)) {
            rowSeries.Points.Clear();
            FillSeriesWithRowValues(rowSeries, row);
            UpdateYCursorInterval();
          }
        }
      }
    }
    #endregion

    #region Chart Events
    private void chart_MouseDown(object sender, MouseEventArgs e) {
      HitTestResult result = chart.HitTest(e.X, e.Y);
      if (result.ChartElementType == ChartElementType.LegendItem) {
        ToggleSeriesVisible(result.Series);
      }
    }

    private void ToggleSeriesVisible(Series series) {
      if (!invisibleSeries.Contains(series)) {
        series.Points.Clear();
        invisibleSeries.Add(series);
      } else {
        invisibleSeries.Remove(series);
        if (Content != null) {

          var row = (from r in Content.Rows
                     where r.Name == series.Name
                     select r).Single();
          FillSeriesWithRowValues(series, row);
          this.chart.Legends[series.Legend].ForeColor = Color.Black;
          UpdateYCursorInterval();
        }
      }
    }

    private void FillSeriesWithRowValues(Series series, DataRow row) {
      for (int i = 0; i < row.Values.Count; i++) {
        var value = row.Values[i];
        DataPoint point = new DataPoint();
        point.XValue = row.VisualProperties.StartIndexZero ? i : i + 1;
        if (IsInvalidValue(value))
          point.IsEmpty = true;
        else
          point.YValues = new double[] { value };
        series.Points.Add(point);
      }
    }

    private void chart_MouseMove(object sender, MouseEventArgs e) {
      HitTestResult result = chart.HitTest(e.X, e.Y);
      if (result.ChartElementType == ChartElementType.LegendItem)
        this.Cursor = Cursors.Hand;
      else
        this.Cursor = Cursors.Default;
    }
    private void chart_CustomizeLegend(object sender, CustomizeLegendEventArgs e) {
      foreach (LegendItem legendItem in e.LegendItems) {
        var series = chart.Series[legendItem.SeriesName];
        if (series != null) {
          bool seriesIsInvisible = invisibleSeries.Contains(series);
          foreach (LegendCell cell in legendItem.Cells) {
            cell.ForeColor = seriesIsInvisible ? Color.Gray : Color.Black;
          }
        }
      }
    }
    #endregion

    private bool IsInvalidValue(double x) {
      return double.IsNaN(x) || x < (double)decimal.MinValue || x > (double)decimal.MaxValue;
    }
  }
}
