#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2011 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Collections;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;

namespace HeuristicLab.Optimization {
  /// <summary>
  /// A run in which an optimizer is executed a given number of times.
  /// </summary>
  [Item("Batch Run", "A run in which an optimizer is executed a given number of times.")]
  [Creatable("Testing & Analysis")]
  [StorableClass]
  public sealed class BatchRun : NamedItem, IOptimizer, IStorableContent {
    public string Filename { get; set; }

    public override Image ItemImage {
      get {
        if (ExecutionState == ExecutionState.Prepared) return HeuristicLab.Common.Resources.VSImageLibrary.BatchRunPrepared;
        else if (ExecutionState == ExecutionState.Started) return HeuristicLab.Common.Resources.VSImageLibrary.BatchRunStarted;
        else if (ExecutionState == ExecutionState.Paused) return HeuristicLab.Common.Resources.VSImageLibrary.BatchRunPaused;
        else if (ExecutionState == ExecutionState.Stopped) return HeuristicLab.Common.Resources.VSImageLibrary.BatchRunStopped;
        else return HeuristicLab.Common.Resources.VSImageLibrary.Event;
      }
    }

    [Storable]
    private ExecutionState executionState;
    public ExecutionState ExecutionState {
      get { return executionState; }
      private set {
        if (executionState != value) {
          executionState = value;
          OnExecutionStateChanged();
          OnItemImageChanged();
        }
      }
    }

    [Storable]
    private TimeSpan executionTime;
    public TimeSpan ExecutionTime {
      get {
        if ((Optimizer != null) && (Optimizer.ExecutionState != ExecutionState.Stopped))
          return executionTime + Optimizer.ExecutionTime;
        else
          return executionTime;
      }
      private set {
        executionTime = value;
        OnExecutionTimeChanged();
      }
    }

    [Storable]
    private IOptimizer optimizer;
    public IOptimizer Optimizer {
      get { return optimizer; }
      set {
        if (optimizer != value) {
          if (optimizer != null) {
            DeregisterOptimizerEvents();
            IEnumerable<IRun> runs = optimizer.Runs;
            optimizer = null; //necessary to avoid removing the runs from the old optimizer
            Runs.RemoveRange(runs);
          }
          optimizer = value;
          if (optimizer != null) {
            RegisterOptimizerEvents();
            Runs.AddRange(optimizer.Runs);
          }
          OnOptimizerChanged();
          Prepare();
        }
      }
    }
    // BackwardsCompatibility3.3
    #region Backwards compatible code (remove with 3.4)
    [Storable(AllowOneWay = true)]
    private IAlgorithm algorithm {
      set { optimizer = value; }
    }
    #endregion

    [Storable]
    private int repetitions;
    public int Repetitions {
      get { return repetitions; }
      set {
        if (repetitions != value) {
          repetitions = value;
          OnRepetitionsChanged();
          if ((Optimizer != null) && (Optimizer.ExecutionState == ExecutionState.Stopped))
            Prepare();
        }
      }
    }
    [Storable]
    private int repetitionsCounter;

    [Storable]
    private RunCollection runs;
    public RunCollection Runs {
      get { return runs; }
      private set {
        if (value == null) throw new ArgumentNullException();
        if (runs != value) {
          if (runs != null) DeregisterRunsEvents();
          runs = value;
          if (runs != null) RegisterRunsEvents();
        }
      }
    }

    public IEnumerable<IOptimizer> NestedOptimizers {
      get {
        if (Optimizer == null) yield break;

        yield return Optimizer;
        foreach (IOptimizer opt in Optimizer.NestedOptimizers)
          yield return opt;
      }
    }

    private bool batchRunStarted = false;
    private bool batchRunPaused = false;
    private bool batchRunStopped = false;

    public BatchRun()
      : base() {
      name = ItemName;
      description = ItemDescription;
      executionState = ExecutionState.Stopped;
      executionTime = TimeSpan.Zero;
      repetitions = 10;
      repetitionsCounter = 0;
      Runs = new RunCollection();
    }
    public BatchRun(string name)
      : base(name) {
      description = ItemDescription;
      executionState = ExecutionState.Stopped;
      executionTime = TimeSpan.Zero;
      repetitions = 10;
      repetitionsCounter = 0;
      Runs = new RunCollection();
    }
    public BatchRun(string name, string description)
      : base(name, description) {
      executionState = ExecutionState.Stopped;
      executionTime = TimeSpan.Zero;
      repetitions = 10;
      repetitionsCounter = 0;
      Runs = new RunCollection();
    }
    [StorableConstructor]
    private BatchRun(bool deserializing) : base(deserializing) { }
    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      Initialize();
    }

    private BatchRun(BatchRun original, Cloner cloner)
      : base(original, cloner) {
      executionState = original.executionState;
      executionTime = original.executionTime;
      optimizer = cloner.Clone(original.optimizer);
      repetitions = original.repetitions;
      repetitionsCounter = original.repetitionsCounter;
      runs = cloner.Clone(original.runs);
      batchRunStarted = original.batchRunStarted;
      batchRunPaused = original.batchRunPaused;
      batchRunStopped = original.batchRunStopped;
      Initialize();
    }
    public override IDeepCloneable Clone(Cloner cloner) {
      if (ExecutionState == ExecutionState.Started) throw new InvalidOperationException(string.Format("Clone not allowed in execution state \"{0}\".", ExecutionState));
      return new BatchRun(this, cloner);
    }

    private void Initialize() {
      if (optimizer != null) RegisterOptimizerEvents();
      if (runs != null) RegisterRunsEvents();
    }

    public void Prepare() {
      Prepare(false);
    }
    public void Prepare(bool clearRuns) {
      if ((ExecutionState != ExecutionState.Prepared) && (ExecutionState != ExecutionState.Paused) && (ExecutionState != ExecutionState.Stopped))
        throw new InvalidOperationException(string.Format("Prepare not allowed in execution state \"{0}\".", ExecutionState));
      if (Optimizer != null) {
        repetitionsCounter = 0;
        if (clearRuns) runs.Clear();
        Optimizer.Prepare(clearRuns);
      } else {
        ExecutionState = ExecutionState.Stopped;
      }
    }
    public void Start() {
      if ((ExecutionState != ExecutionState.Prepared) && (ExecutionState != ExecutionState.Paused))
        throw new InvalidOperationException(string.Format("Start not allowed in execution state \"{0}\".", ExecutionState));
      if (Optimizer == null) return;

      batchRunStarted = true;
      batchRunPaused = false;
      batchRunStopped = false;
      if (Optimizer.ExecutionState != ExecutionState.Prepared) Optimizer.Prepare();
      Optimizer.Start();
    }
    public void Pause() {
      if (ExecutionState != ExecutionState.Started)
        throw new InvalidOperationException(string.Format("Pause not allowed in execution state \"{0}\".", ExecutionState));
      if (Optimizer == null) return;
      if (Optimizer.ExecutionState != ExecutionState.Started) return;

      batchRunStarted = false;
      batchRunPaused = true;
      batchRunStopped = false;
      Optimizer.Pause();
    }
    public void Stop() {
      if ((ExecutionState != ExecutionState.Started) && (ExecutionState != ExecutionState.Paused))
        throw new InvalidOperationException(string.Format("Stop not allowed in execution state \"{0}\".", ExecutionState));
      if (Optimizer == null) return;
      if (Optimizer.ExecutionState != ExecutionState.Started && Optimizer.ExecutionState != ExecutionState.Paused) return;
      batchRunStarted = false;
      batchRunPaused = false;
      batchRunStopped = true;
      Optimizer.Stop();
    }

    #region Events
    public event EventHandler ExecutionStateChanged;
    private void OnExecutionStateChanged() {
      EventHandler handler = ExecutionStateChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler ExecutionTimeChanged;
    private void OnExecutionTimeChanged() {
      EventHandler handler = ExecutionTimeChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler OptimizerChanged;
    private void OnOptimizerChanged() {
      EventHandler handler = OptimizerChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler RepetitionsChanged;
    private void OnRepetitionsChanged() {
      EventHandler handler = RepetitionsChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler Prepared;
    private void OnPrepared() {
      ExecutionState = ExecutionState.Prepared;
      EventHandler handler = Prepared;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler Started;
    private void OnStarted() {
      ExecutionState = ExecutionState.Started;
      EventHandler handler = Started;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler Paused;
    private void OnPaused() {
      ExecutionState = ExecutionState.Paused;
      EventHandler handler = Paused;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler Stopped;
    private void OnStopped() {
      ExecutionState = ExecutionState.Stopped;
      EventHandler handler = Stopped;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler<EventArgs<Exception>> ExceptionOccurred;
    private void OnExceptionOccurred(Exception exception) {
      EventHandler<EventArgs<Exception>> handler = ExceptionOccurred;
      if (handler != null) handler(this, new EventArgs<Exception>(exception));
    }

    private void RegisterOptimizerEvents() {
      optimizer.ExceptionOccurred += new EventHandler<EventArgs<Exception>>(Optimizer_ExceptionOccurred);
      optimizer.ExecutionTimeChanged += new EventHandler(Optimizer_ExecutionTimeChanged);
      optimizer.Paused += new EventHandler(Optimizer_Paused);
      optimizer.Prepared += new EventHandler(Optimizer_Prepared);
      optimizer.Started += new EventHandler(Optimizer_Started);
      optimizer.Stopped += new EventHandler(Optimizer_Stopped);
      optimizer.Runs.CollectionReset += new CollectionItemsChangedEventHandler<IRun>(Optimizer_Runs_CollectionReset);
      optimizer.Runs.ItemsAdded += new CollectionItemsChangedEventHandler<IRun>(Optimizer_Runs_ItemsAdded);
      optimizer.Runs.ItemsRemoved += new CollectionItemsChangedEventHandler<IRun>(Optimizer_Runs_ItemsRemoved);
    }
    private void DeregisterOptimizerEvents() {
      optimizer.ExceptionOccurred -= new EventHandler<EventArgs<Exception>>(Optimizer_ExceptionOccurred);
      optimizer.ExecutionTimeChanged -= new EventHandler(Optimizer_ExecutionTimeChanged);
      optimizer.Paused -= new EventHandler(Optimizer_Paused);
      optimizer.Prepared -= new EventHandler(Optimizer_Prepared);
      optimizer.Started -= new EventHandler(Optimizer_Started);
      optimizer.Stopped -= new EventHandler(Optimizer_Stopped);
      optimizer.Runs.CollectionReset -= new CollectionItemsChangedEventHandler<IRun>(Optimizer_Runs_CollectionReset);
      optimizer.Runs.ItemsAdded -= new CollectionItemsChangedEventHandler<IRun>(Optimizer_Runs_ItemsAdded);
      optimizer.Runs.ItemsRemoved -= new CollectionItemsChangedEventHandler<IRun>(Optimizer_Runs_ItemsRemoved);
    }
    private void Optimizer_ExceptionOccurred(object sender, EventArgs<Exception> e) {
      OnExceptionOccurred(e.Value);
    }
    private void Optimizer_ExecutionTimeChanged(object sender, EventArgs e) {
      OnExecutionTimeChanged();
    }
    private void Optimizer_Paused(object sender, EventArgs e) {
      if (ExecutionState == ExecutionState.Started) {
        batchRunStarted = false;
        batchRunPaused = true;
        batchRunStopped = false;
        OnPaused();
      }
    }
    private void Optimizer_Prepared(object sender, EventArgs e) {
      if (ExecutionState == ExecutionState.Stopped || !batchRunStarted) {
        OnPrepared();
      }
    }
    private void Optimizer_Started(object sender, EventArgs e) {
      if (ExecutionState != ExecutionState.Started)
        OnStarted();
    }
    private void Optimizer_Stopped(object sender, EventArgs e) {
      repetitionsCounter++;

      if (batchRunStopped) OnStopped();
      else if (repetitionsCounter >= repetitions) OnStopped();
      else if (batchRunPaused) OnPaused();
      else if (batchRunStarted) {
        Optimizer.Prepare();
        Optimizer.Start();
      } else OnStopped();
    }
    private void Optimizer_Runs_CollectionReset(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      Runs.RemoveRange(e.OldItems);
      Runs.AddRange(e.Items);
    }
    private void Optimizer_Runs_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      Runs.AddRange(e.Items);
    }
    private void Optimizer_Runs_ItemsRemoved(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      Runs.RemoveRange(e.Items);
    }

    private void RegisterRunsEvents() {
      runs.CollectionReset += new CollectionItemsChangedEventHandler<IRun>(Runs_CollectionReset);
      runs.ItemsAdded += new CollectionItemsChangedEventHandler<IRun>(Runs_ItemsAdded);
      runs.ItemsRemoved += new CollectionItemsChangedEventHandler<IRun>(Runs_ItemsRemoved);
    }

    private void DeregisterRunsEvents() {
      runs.CollectionReset -= new CollectionItemsChangedEventHandler<IRun>(Runs_CollectionReset);
      runs.ItemsAdded -= new CollectionItemsChangedEventHandler<IRun>(Runs_ItemsAdded);
      runs.ItemsRemoved -= new CollectionItemsChangedEventHandler<IRun>(Runs_ItemsRemoved);
    }
    private void Runs_CollectionReset(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      foreach (IRun run in e.OldItems) {
        IItem item;
        run.Results.TryGetValue("Execution Time", out item);
        TimeSpanValue executionTime = item as TimeSpanValue;
        if (executionTime != null) ExecutionTime -= executionTime.Value;
      }
      if (Optimizer != null) Optimizer.Runs.RemoveRange(e.OldItems);
      foreach (IRun run in e.Items) {
        IItem item;
        run.Results.TryGetValue("Execution Time", out item);
        TimeSpanValue executionTime = item as TimeSpanValue;
        if (executionTime != null) ExecutionTime += executionTime.Value;
      }
    }
    private void Runs_ItemsAdded(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      foreach (IRun run in e.Items) {
        IItem item;
        run.Results.TryGetValue("Execution Time", out item);
        TimeSpanValue executionTime = item as TimeSpanValue;
        if (executionTime != null) ExecutionTime += executionTime.Value;
      }
    }
    private void Runs_ItemsRemoved(object sender, CollectionItemsChangedEventArgs<IRun> e) {
      foreach (IRun run in e.Items) {
        IItem item;
        run.Results.TryGetValue("Execution Time", out item);
        TimeSpanValue executionTime = item as TimeSpanValue;
        if (executionTime != null) ExecutionTime -= executionTime.Value;
      }
      if (Optimizer != null) Optimizer.Runs.RemoveRange(e.Items);
    }
    #endregion
  }
}
