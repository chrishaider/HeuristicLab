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
using HeuristicLab.Collections;
using HeuristicLab.MainForm;

namespace HeuristicLab.Core.Views {
  [View("CheckedItemCollection View")]
  [Content(typeof(ICheckedItemCollection<>), true)]
  [Content(typeof(CheckedItemCollection<>), true)]
  public partial class CheckedItemCollectionView<T> : ItemCollectionView<T> where T : class, IItem {
    public new ICheckedItemCollection<T> Content {
      get { return (ICheckedItemCollection<T>)base.Content; }
      set { base.Content = value; }
    }
    
    public CheckedItemCollectionView()
      : base() {
      InitializeComponent();
    }

    protected override void RegisterContentEvents() {
      Content.CheckedItemsChanged += new HeuristicLab.Collections.CollectionItemsChangedEventHandler<T>(Content_CheckedItemsChanged);
      base.RegisterContentEvents();
    }

    protected override void DeregisterContentEvents() {
      Content.CheckedItemsChanged -= new HeuristicLab.Collections.CollectionItemsChangedEventHandler<T>(Content_CheckedItemsChanged);
      base.DeregisterContentEvents();
    }

    protected override void SetEnabledStateOfControls() {
      base.SetEnabledStateOfControls();
      base.itemsListView.Enabled = !this.Locked;
    }

    protected override ListViewItem CreateListViewItem(T item) {
      ListViewItem listViewItem = base.CreateListViewItem(item);
      listViewItem.Checked = Content.ItemChecked(item);
      return listViewItem;
    }

    #region ListView Events
    private bool doubleClick;
    protected virtual void itemsListView_ItemCheck(object sender, ItemCheckEventArgs e) {
      if (doubleClick) {
        e.NewValue = e.CurrentValue;
        doubleClick = false;
      } else {
        var checkedItem = (T)itemsListView.Items[e.Index].Tag;
        bool check = e.NewValue == CheckState.Checked;
        if (Content.ItemChecked(checkedItem) != check) {
          Content.SetItemCheckedState(checkedItem, check);
        }
      }
    }
    protected void itemsListView_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e) {
      if (e.Clicks > 1)
        doubleClick = true;
    }
    #endregion

    #region Content Events
    protected virtual void Content_CheckedItemsChanged(object sender, HeuristicLab.Collections.CollectionItemsChangedEventArgs<T> e) {
      if (InvokeRequired)
        Invoke(new CollectionItemsChangedEventHandler<T>(Content_CheckedItemsChanged), sender, e);
      else {
        foreach (T item in e.Items) {
          foreach (ListViewItem listViewItem in GetListViewItemsForItem(item)) {
            if (listViewItem.Checked != Content.ItemChecked(item))
              listViewItem.Checked = Content.ItemChecked(item);
          }
        }
      }
    }      
    #endregion
  }
}
