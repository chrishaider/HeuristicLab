#region License Information
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

using System;
using System.ComponentModel;
using System.Windows.Forms;
using HeuristicLab.Core.Views;
using HeuristicLab.MainForm;

namespace HeuristicLab.Problems.TravelingSalesman.Views {
  /// <summary>
  /// The base class for visual representations of a path tour for a TSP.
  /// </summary>
  [View("TSP Solution View")]
  [Content(typeof(ITSPSolution), true)]
  public partial class TSPSolutionView : ItemView, ITSPVisualizerView {
    public TSPVisualizer Visualizer { get; set; }

    public new ITSPSolution Content {
      get { return (ITSPSolution)base.Content; }
      set { base.Content = value; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="TSPSolutionView"/>.
    /// </summary>
    public TSPSolutionView() {
      InitializeComponent();
      Visualizer = new TSPVisualizer();
    }

    protected override void DeregisterContentEvents() {
      Content.PropertyChanged -= ContentOnPropertyChanged;
      base.DeregisterContentEvents();
    }
    protected override void RegisterContentEvents() {
      base.RegisterContentEvents();
      Content.PropertyChanged += ContentOnPropertyChanged;
    }

    protected override void OnContentChanged() {
      base.OnContentChanged();
      if (Content == null) {
        qualityViewHost.Content = null;
        pictureBox.Image = null;
        tourViewHost.Content = null;
        Visualizer.Coordinates = null;
        Visualizer.Tour = null;
      } else {
        qualityViewHost.Content = Content.TourLength;
        if (Content.TSPData is CoordinatesTSPData coordTsp)
          Visualizer.Coordinates = coordTsp.Coordinates;
        else if (Content.TSPData is MatrixTSPData matrixTsp)
          Visualizer.Coordinates = matrixTsp.DisplayCoordinates;
        Visualizer.Tour = Content.Tour;
        GenerateImage();
        tourViewHost.Content = Content.Tour;
      }
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      qualityGroupBox.Enabled = Content != null;
      pictureBox.Enabled = Content != null;
      tourGroupBox.Enabled = Content != null;
    }

    protected virtual void GenerateImage() {
      if ((pictureBox.Width > 0) && (pictureBox.Height > 0)) {
        if (Content == null) {
          pictureBox.Image = null;
        } else {
          pictureBox.Image = Visualizer.Draw(pictureBox.Width, pictureBox.Height);
        }
      }
    }

    protected virtual void ContentOnPropertyChanged(object sender, PropertyChangedEventArgs e) {
      if (InvokeRequired)
        Invoke((Action<object, PropertyChangedEventArgs>)ContentOnPropertyChanged, sender, e);
      else {
        switch (e.PropertyName) {
          case nameof(Content.TSPData):
            if (Content.TSPData is CoordinatesTSPData coordTsp)
              Visualizer.Coordinates = coordTsp.Coordinates;
            else if (Content.TSPData is MatrixTSPData matrixTsp)
              Visualizer.Coordinates = matrixTsp.DisplayCoordinates;
            GenerateImage();
            break;
          case nameof(Content.Tour):
            Visualizer.Tour = Content.Tour;
            GenerateImage();
            tourViewHost.Content = Content.Tour;
            break;
          case nameof(Content.TourLength):
            qualityViewHost.Content = Content.TourLength;
            break;
        }
      }
    }

    private void pictureBox_SizeChanged(object sender, EventArgs e) {
      GenerateImage();
    }
  }
}
