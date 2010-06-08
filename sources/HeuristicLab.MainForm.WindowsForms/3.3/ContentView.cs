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
using HeuristicLab.Common;
using System.Reflection;

namespace HeuristicLab.MainForm.WindowsForms {
  public partial class ContentView : View, IContentView {
    public ContentView()
      : base() {
      InitializeComponent();
      this.locked = false;
    }

    private IContent content;
    public IContent Content {
      get { return content; }
      set {
        if ((value != null) && (!MainFormManager.ViewCanViewContent(this, value)))
          throw new ArgumentException(string.Format("View \"{0}\" cannot view object \"{1}\".", this.GetType().Name, value.GetType().Name));
        if (InvokeRequired) {
          Invoke(new Action<IContent>(delegate(IContent o) { this.Content = o; }), value);
        } else {
          if (this.content != value) {
            this.SuspendRepaint();
            if (this.content != null) this.DeregisterContentEvents();
            this.content = value;
            if (this.content != null) this.RegisterContentEvents();
            this.OnContentChanged();
            this.SetEnabledStateOfControls();
            this.OnChanged();
            this.ResumeRepaint(true);
          }
        }
      }
    }

    private bool locked;
    public virtual bool Locked {
      get { return this.locked; }
      set {
        if (InvokeRequired) {
          Action<bool> action = delegate(bool b) { this.Locked = b; };
          Invoke(action, value);
        } else {
          if (value != locked) {
            this.SuspendRepaint();
            locked = value;
            OnLockedChanged();
            this.SetEnabledStateOfControls();
            PropertyInfo prop = typeof(IContentView).GetProperty("Locked");
            PropagateStateChanges(this, typeof(IContentView), prop);
            OnChanged();
            this.ResumeRepaint(true);
          }
        }
      }
    }
    public event EventHandler LockedChanged;
    protected virtual void OnLockedChanged() {
      if (InvokeRequired)
        Invoke((MethodInvoker)OnLockedChanged);
      else {
        EventHandler handler = LockedChanged;
        if (handler != null)
          handler(this, EventArgs.Empty);
      }
    }
    /// <summary>
    /// Adds eventhandlers to the current instance.
    /// </summary>
    protected virtual void RegisterContentEvents() {
    }

    /// <summary>
    /// Removes the eventhandlers from the current instance.
    /// </summary>
    protected virtual void DeregisterContentEvents() {
    }

    /// <summary>
    /// Is called when the content property changes.
    /// </summary>
    protected virtual void OnContentChanged() {
    }

    /// <summary>
    /// This method is called if the ReadyOnly, Locked or Content property of the ContentView changes to update the controls of the view.
    /// </summary>
    protected override void SetEnabledStateOfControls() {
    }
  }
}
