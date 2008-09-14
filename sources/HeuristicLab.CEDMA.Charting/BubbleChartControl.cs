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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.Text;
using System.Windows.Forms;
using HeuristicLab.Charting;

namespace HeuristicLab.CEDMA.Charting {
  public partial class BubbleChartControl : UserControl {
    private Bitmap bitmap;
    private bool renderingRequired;
    private Point mousePosition;
    private Record clickedRecord;
    private Point buttonDownPoint;

    private BubbleChart myChart;
    public BubbleChart Chart {
      get { return myChart; }
      set {
        if(myChart != null) myChart.Update -= new EventHandler(myChart_Update);
        myChart = value;
        if(myChart != null) {
          myChart.Update += new EventHandler(myChart_Update);
          SetMode(Chart.Mode);
        }
        GenerateImage();
      }
    }
    private bool myScaleOnResize;
    public bool ScaleOnResize {
      get { return myScaleOnResize; }
      set { myScaleOnResize = value; }
    }

    public BubbleChartControl() {
      InitializeComponent();
      myScaleOnResize = true;
      GenerateImage();
    }

    private void myChart_Update(object sender, EventArgs e) {
      GenerateImage();
    }
    private void pictureBox_SizeChanged(object sender, EventArgs e) {
      if(ScaleOnResize) {
        if((pictureBox.Width > 0) && (pictureBox.Height > 0) && (Chart != null)) {
          PointD point = Chart.TransformPixelToWorld(new Point(pictureBox.Width, Chart.SizeInPixels.Height - pictureBox.Height));
          Chart.SetPosition(Chart.LowerLeft, point);
        }
      }
      GenerateImage();
    }
    private void pictureBox_VisibleChanged(object sender, EventArgs e) {
      if(pictureBox.Visible && renderingRequired) {
        GenerateImage();
      }
    }

    private void pictureBox_MouseDown(object sender, MouseEventArgs e) {
      clickedRecord = null;
      buttonDownPoint = e.Location;
      if(e.Button == MouseButtons.Left || e.Button == MouseButtons.Right) {
        clickedRecord = Chart.GetRecord(buttonDownPoint);
      }
    }
    private void pictureBox_MouseUp(object sender, MouseEventArgs e) {
      if(e.Button == MouseButtons.Left) {
        Point lowerLeft = new Point(Math.Min(e.X, buttonDownPoint.X),
                                    Math.Max(e.Y, buttonDownPoint.Y));
        Point upperRight = new Point(Math.Max(e.X, buttonDownPoint.X),
                                     Math.Min(e.Y, buttonDownPoint.Y));
        if(Chart.Mode == ChartMode.Zoom) {
          pictureBox.Refresh();
          if((lowerLeft.X != upperRight.X) && (lowerLeft.Y != upperRight.Y)) {
            Chart.ZoomIn(lowerLeft, upperRight);
          }
        } else if(Chart.Mode == ChartMode.Select) {
          if((lowerLeft.X != upperRight.X) && (lowerLeft.Y != upperRight.Y)) {
            Chart.MouseDrag(lowerLeft, upperRight, e.Button);
          }
        } else if(Chart.Mode == ChartMode.Move) {
        }
      } else if(e.Button == MouseButtons.Middle) {
        if(Chart.Mode == ChartMode.Zoom) {
          Chart.ZoomOut();
        }
      }
    }
    private void pictureBox_MouseMove(object sender, MouseEventArgs e) {
      toolTip.SetToolTip(pictureBox, Chart.GetToolTipText(e.Location));
      Cursor cursor = Chart.GetCursor(e.Location);
      if(cursor != null) pictureBox.Cursor = cursor;
      else pictureBox.Cursor = Cursors.Default;

      if(e.Button != MouseButtons.None) {
        if((Chart.Mode == ChartMode.Zoom || Chart.Mode == ChartMode.Select) && (e.Button == MouseButtons.Left)) {
          pictureBox.Refresh();
          Graphics graphics = pictureBox.CreateGraphics();
          Pen pen = new Pen(Color.Gray);
          pen.DashStyle = DashStyle.Dash;
          graphics.DrawRectangle(pen,
                                 Math.Min(e.X, buttonDownPoint.X),
                                 Math.Min(e.Y, buttonDownPoint.Y),
                                 Math.Abs(e.X - buttonDownPoint.X),
                                 Math.Abs(e.Y - buttonDownPoint.Y));
          pen.Dispose();
          graphics.Dispose();
        }
      }
      mousePosition = e.Location;
    }

    private void zoomToolStripMenuItem_Click(object sender, EventArgs e) {
      SetMode(ChartMode.Zoom);
    }
    private void selectToolStripMenuItem_Click(object sender, EventArgs e) {
      SetMode(ChartMode.Select);
    }
    private void moveToolStripMenuItem_Click(object sender, EventArgs e) {
      SetMode(ChartMode.Move);
    }

    private void GenerateImage() {
      if(!Visible) {
        renderingRequired = true;
      } else {
        if((pictureBox.Width == 0) || (pictureBox.Height == 0)) {
          bitmap = null;
        } else {
          bitmap = new Bitmap(pictureBox.Width, pictureBox.Height);
          if(Chart != null) {
            Graphics graphics = Graphics.FromImage(bitmap);
            graphics.SetClip(new System.Drawing.Rectangle(0, 0, pictureBox.Width, pictureBox.Height));
            Chart.Render(graphics, pictureBox.Width, pictureBox.Height);
            graphics.Dispose();
          }
        }
        pictureBox.Image = bitmap;
        renderingRequired = false;
      }
    }

    private void SetMode(ChartMode mode) {
      zoomToolStripMenuItem.Checked = false;
      selectToolStripMenuItem.Checked = false;
      moveToolStripMenuItem.Checked = false;
      if(mode == ChartMode.Zoom) zoomToolStripMenuItem.Checked = true;
      else if(mode == ChartMode.Select) selectToolStripMenuItem.Checked = true;
      else if(mode == ChartMode.Move) moveToolStripMenuItem.Checked = true;
      Chart.Mode = mode;
    }

    private void pictureBox_MouseClick(object sender, MouseEventArgs e) {
      if(clickedRecord != null) clickedRecord.ToggleSelected();
    }

    private void pictureBox_MouseDoubleClick(object sender, MouseEventArgs e) {
      if(clickedRecord != null) clickedRecord.OpenModel();
    }
  }
}
