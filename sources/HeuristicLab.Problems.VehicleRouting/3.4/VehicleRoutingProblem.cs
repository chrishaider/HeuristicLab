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
using System.Drawing;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.PluginInfrastructure;
using HeuristicLab.Problems.Instances;
using HeuristicLab.Problems.VehicleRouting.Interfaces;
using HeuristicLab.Problems.VehicleRouting.Interpreters;
using HeuristicLab.Problems.VehicleRouting.ProblemInstances;
using HeuristicLab.Problems.VehicleRouting.Variants;

namespace HeuristicLab.Problems.VehicleRouting {
  [Item("Vehicle Routing Problem", "Represents a Vehicle Routing Problem.")]
  [Creatable("Problems")]
  [StorableClass]
  public sealed class VehicleRoutingProblem : ParameterizedNamedItem, ISingleObjectiveHeuristicOptimizationProblem, IStorableContent, IProblemInstanceConsumer<IVRPData> {
    public string Filename { get; set; }

    public static new Image StaticItemImage {
      get { return HeuristicLab.Common.Resources.VSImageLibrary.Type; }
    }

    #region Parameter Properties
    public ValueParameter<BoolValue> MaximizationParameter {
      get { return (ValueParameter<BoolValue>)Parameters["Maximization"]; }
    }
    IParameter ISingleObjectiveHeuristicOptimizationProblem.MaximizationParameter {
      get { return MaximizationParameter; }
    }
    public ValueParameter<IVRPProblemInstance> ProblemInstanceParameter {
      get { return (ValueParameter<IVRPProblemInstance>)Parameters["ProblemInstance"]; }
    }
    public OptionalValueParameter<DoubleValue> BestKnownQualityParameter {
      get { return (OptionalValueParameter<DoubleValue>)Parameters["BestKnownQuality"]; }
    }
    IParameter ISingleObjectiveHeuristicOptimizationProblem.BestKnownQualityParameter {
      get { return BestKnownQualityParameter; }
    }
    public OptionalValueParameter<VRPSolution> BestKnownSolutionParameter {
      get { return (OptionalValueParameter<VRPSolution>)Parameters["BestKnownSolution"]; }
    }
    public IValueParameter<IVRPCreator> SolutionCreatorParameter {
      get { return (IValueParameter<IVRPCreator>)Parameters["SolutionCreator"]; }
    }
    IParameter IHeuristicOptimizationProblem.SolutionCreatorParameter {
      get { return SolutionCreatorParameter; }
    }
    public IValueParameter<IVRPEvaluator> EvaluatorParameter {
      get { return (IValueParameter<IVRPEvaluator>)Parameters["Evaluator"]; }
    }
    IParameter IHeuristicOptimizationProblem.EvaluatorParameter {
      get { return EvaluatorParameter; }
    }
    #endregion

    #region Properties
    public IVRPProblemInstance ProblemInstance {
      get { return ProblemInstanceParameter.Value; }
      set { ProblemInstanceParameter.Value = value; }
    }

    public VRPSolution BestKnownSolution {
      get { return BestKnownSolutionParameter.Value; }
      set { BestKnownSolutionParameter.Value = value; }
    }

    public DoubleValue BestKnownQuality {
      get { return BestKnownQualityParameter.Value; }
      set { BestKnownQualityParameter.Value = value; }
    }

    public ISingleObjectiveEvaluator Evaluator {
      get { return EvaluatorParameter.Value; }
    }

    IEvaluator IHeuristicOptimizationProblem.Evaluator {
      get { return this.Evaluator; }
    }

    public ISolutionCreator SolutionCreator {
      get { return SolutionCreatorParameter.Value; }
    }

    [Storable]
    private List<IOperator> operators;

    public IEnumerable<IOperator> Operators {
      get { return operators; }
    }
    #endregion

    [StorableConstructor]
    private VehicleRoutingProblem(bool deserializing) : base(deserializing) { }
    public VehicleRoutingProblem()
      : base() {
      Parameters.Add(new ValueParameter<BoolValue>("Maximization", "Set to false as the Vehicle Routing Problem is a minimization problem.", new BoolValue(false)));
      Parameters.Add(new ValueParameter<IVRPProblemInstance>("ProblemInstance", "The VRP problem instance"));
      Parameters.Add(new OptionalValueParameter<DoubleValue>("BestKnownQuality", "The quality of the best known solution of this VRP instance."));
      Parameters.Add(new OptionalValueParameter<VRPSolution>("BestKnownSolution", "The best known solution of this VRP instance."));

      Parameters.Add(new ConstrainedValueParameter<IVRPCreator>("SolutionCreator", "The operator which should be used to create new VRP solutions."));
      Parameters.Add(new ValueParameter<IVRPEvaluator>("Evaluator", "The operator which should be used to evaluate VRP solutions."));

      EvaluatorParameter.Hidden = true;

      operators = new List<IOperator>();

      InitializeRandomVRPInstance();
      InitializeOperators();

      AttachEventHandlers();
      AttachProblemInstanceEventHandlers();
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      cloner.Clone(ProblemInstance);
      return new VehicleRoutingProblem(this, cloner);
    }

    private VehicleRoutingProblem(VehicleRoutingProblem original, Cloner cloner)
      : base(original, cloner) {
      this.operators = original.operators.Select(x => (IOperator)cloner.Clone(x)).ToList();
      this.AttachEventHandlers();
    }

    #region Events
    public event EventHandler SolutionCreatorChanged;
    private void OnSolutionCreatorChanged() {
      EventHandler handler = SolutionCreatorChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler EvaluatorChanged;
    private void OnEvaluatorChanged() {
      EventHandler handler = EvaluatorChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler OperatorsChanged;
    private void OnOperatorsChanged() {
      EventHandler handler = OperatorsChanged;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    public event EventHandler Reset;
    private void OnReset() {
      EventHandler handler = Reset;
      if (handler != null) handler(this, EventArgs.Empty);
    }
    #endregion

    #region Helpers
    [StorableHook(HookType.AfterDeserialization)]
    private void AfterDeserialization() {
      AttachEventHandlers();
      AttachProblemInstanceEventHandlers();
    }

    private void AttachEventHandlers() {
      ProblemInstanceParameter.ValueChanged += new EventHandler(ProblemInstanceParameter_ValueChanged);
      BestKnownSolutionParameter.ValueChanged += new EventHandler(BestKnownSolutionParameter_ValueChanged);
    }

    private void AttachProblemInstanceEventHandlers() {
      var solutionCreatorParameter = SolutionCreatorParameter as ConstrainedValueParameter<IVRPCreator>;
      solutionCreatorParameter.ValidValues.Clear();

      if (ProblemInstance != null) {
        EvaluatorParameter.Value = ProblemInstance.SolutionEvaluator;
        IVRPCreator defaultCreator = null;
        foreach (IVRPCreator creator in operators.Where(o => o is IVRPCreator)) {
          solutionCreatorParameter.ValidValues.Add(creator);
          if (creator is Encodings.Alba.RandomCreator)
            defaultCreator = creator;
        }
        if (defaultCreator != null)
          solutionCreatorParameter.Value = defaultCreator;

        ProblemInstance.EvaluationChanged += new EventHandler(ProblemInstance_EvaluationChanged);
      }
    }

    private void EvalBestKnownSolution() {
      if (BestKnownSolution != null) {
        //call evaluator
        BestKnownQuality = new DoubleValue(ProblemInstance.Evaluate(BestKnownSolution.Solution).Quality);
        BestKnownSolution.Quality = BestKnownQuality;
      } else {
        BestKnownQuality = null;
      }
    }

    void BestKnownSolutionParameter_ValueChanged(object sender, EventArgs e) {
      EvalBestKnownSolution();
    }

    void ProblemInstance_EvaluationChanged(object sender, EventArgs e) {
      EvalBestKnownSolution();
    }

    void ProblemInstanceParameter_ValueChanged(object sender, EventArgs e) {
      InitializeOperators();
      AttachProblemInstanceEventHandlers();

      OnSolutionCreatorChanged();
      OnEvaluatorChanged();
      OnOperatorsChanged();
    }

    public void SetProblemInstance(IVRPProblemInstance instance) {
      ProblemInstanceParameter.ValueChanged -= new EventHandler(ProblemInstanceParameter_ValueChanged);

      ProblemInstance = instance;
      AttachProblemInstanceEventHandlers();

      OnSolutionCreatorChanged();
      OnEvaluatorChanged();

      ProblemInstanceParameter.ValueChanged += new EventHandler(ProblemInstanceParameter_ValueChanged);
    }

    private void SolutionCreatorParameter_ValueChanged(object sender, EventArgs e) {
      ParameterizeSolutionCreator();

      OnSolutionCreatorChanged();
    }
    private void EvaluatorParameter_ValueChanged(object sender, EventArgs e) {
      OnEvaluatorChanged();
    }

    private void InitializeOperators() {
      operators = new List<IOperator>();

      if (ProblemInstance != null) {
        operators.AddRange(
        ProblemInstance.Operators.Concat(
          ApplicationManager.Manager.GetInstances<IGeneralVRPOperator>().Cast<IOperator>()).OrderBy(op => op.Name));
      }

      ParameterizeOperators();
    }

    private void ParameterizeSolutionCreator() {
      if (SolutionCreator is IMultiVRPOperator) {
        (SolutionCreator as IMultiVRPOperator).SetOperators(Operators);
      }
    }

    private void ParameterizeOperators() {
      foreach (IOperator op in Operators) {
        if (op is IMultiVRPOperator) {
          (op as IMultiVRPOperator).SetOperators(Operators);
        }
      }
    }
    #endregion

    private void InitializeRandomVRPInstance() {
      System.Random rand = new System.Random();

      CVRPTWProblemInstance problem = new CVRPTWProblemInstance();
      int cities = 100;

      problem.Coordinates = new DoubleMatrix(cities + 1, 2);
      problem.Demand = new DoubleArray(cities + 1);
      problem.DueTime = new DoubleArray(cities + 1);
      problem.ReadyTime = new DoubleArray(cities + 1);
      problem.ServiceTime = new DoubleArray(cities + 1);

      problem.Vehicles.Value = 100;
      problem.Capacity.Value = 200;

      for (int i = 0; i <= cities; i++) {
        problem.Coordinates[i, 0] = rand.Next(0, 100);
        problem.Coordinates[i, 1] = rand.Next(0, 100);

        if (i == 0) {
          problem.Demand[i] = 0;
          problem.DueTime[i] = Int16.MaxValue;
          problem.ReadyTime[i] = 0;
          problem.ServiceTime[i] = 0;
        } else {
          problem.Demand[i] = rand.Next(10, 50);
          problem.DueTime[i] = rand.Next((int)Math.Ceiling(problem.GetDistance(0, i, null)), 1200);
          problem.ReadyTime[i] = problem.DueTime[i] - rand.Next(0, 100);
          problem.ServiceTime[i] = 90;
        }
      }

      this.ProblemInstance = problem;
    }

    public void ImportSolution(string solutionFileName) {
      SolutionParser parser = new SolutionParser(solutionFileName);
      parser.Parse();

      HeuristicLab.Problems.VehicleRouting.Encodings.Potvin.PotvinEncoding encoding = new Encodings.Potvin.PotvinEncoding(ProblemInstance);

      int cities = 0;
      foreach (List<int> route in parser.Routes) {
        Tour tour = new Tour();
        tour.Stops.AddRange(route);
        cities += tour.Stops.Count;

        encoding.Tours.Add(tour);
      }

      if (cities != ProblemInstance.Coordinates.Rows - 1)
        ErrorHandling.ShowErrorDialog(new Exception("The optimal solution does not seem to correspond with the problem data"));
      else {
        VRPSolution solution = new VRPSolution(ProblemInstance, encoding, new DoubleValue(0));
        BestKnownSolutionParameter.Value = solution;
      }
    }

    public void Load(IVRPData data) {
      Type interpreterType = typeof(IVRPDataInterpreter<>).MakeGenericType(data.GetType());
      var interpreters = ApplicationManager.Manager.GetInstances(interpreterType);
      if (interpreters.Count() > 0) {
        IVRPDataInterpreter interpreter = interpreters.First() as IVRPDataInterpreter;
        VRPInstanceDescription instance = interpreter.Interpret(data);

        Name = instance.Name;
        Description = instance.Description;
        if (ProblemInstance != null && instance.ProblemInstance != null &&
          instance.ProblemInstance.GetType() == ProblemInstance.GetType())
          SetProblemInstance(instance.ProblemInstance);
        else
          ProblemInstance = instance.ProblemInstance;

        OnReset();
        BestKnownQuality = null;
        BestKnownSolution = null;

        if (instance.BestKnownQuality != null) {
          BestKnownQuality = new DoubleValue((double)instance.BestKnownQuality);
        }

        if (instance.BestKnownSolution != null) {
          VRPSolution solution = new VRPSolution(ProblemInstance, instance.BestKnownSolution, new DoubleValue(0));
          BestKnownSolution = solution;
        }
      } else {
        throw new Exception("Cannot find an interpreter for " + data.GetType());
      }
    }
  }
}
