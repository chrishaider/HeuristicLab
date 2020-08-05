﻿#region License Information
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

using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Optimization;
using HeuristicLab.Parameters;
using HeuristicLab.Problems.VehicleRouting.Interfaces;
using HeuristicLab.Problems.VehicleRouting.ProblemInstances;

namespace HeuristicLab.Problems.VehicleRouting {
  /// <summary>
  /// An operator for adaptive constraint relaxation.
  /// </summary>
  [Item("TimeWindowRelaxationVRPAnalyzer", "An operator for adaptively relaxing the time window constraints.")]
  [StorableType("e782fe43-c77f-445a-a676-7b03db53ca99")]
  public class TimeWindowRelaxationVRPAnalyzer : SingleSuccessorOperator, IAnalyzer, ITimeWindowedOperator {
    [Storable] public ILookupParameter<IVRPProblemInstance> ProblemInstanceParameter { get; private set; }
    [Storable] public IScopeTreeLookupParameter<IVRPEncodedSolution> VRPToursParameter { get; private set; }
    [Storable] public IScopeTreeLookupParameter<CVRPTWEvaluation> EvaluationResultParameter { get; private set; }

    [Storable] public IValueParameter<DoubleValue> SigmaParameter { get; private set; }
    [Storable] public IValueParameter<DoubleValue> PhiParameter { get; private set; }
    [Storable] public IValueParameter<DoubleValue> MinPenaltyFactorParameter { get; private set; }
    [Storable] public IValueParameter<DoubleValue> MaxPenaltyFactorParameter { get; private set; }

    [Storable] public IResultParameter<DoubleValue> CurrentTardinessPenaltyResult { get; private set; }

    public bool EnabledByDefault {
      get { return false; }
    }

    [StorableConstructor]
    protected TimeWindowRelaxationVRPAnalyzer(StorableConstructorFlag _) : base(_) { }
    protected TimeWindowRelaxationVRPAnalyzer(TimeWindowRelaxationVRPAnalyzer original, Cloner cloner)
      : base(original, cloner) {
      ProblemInstanceParameter = cloner.Clone(original.ProblemInstanceParameter);
      VRPToursParameter = cloner.Clone(original.VRPToursParameter);
      EvaluationResultParameter = cloner.Clone(original.EvaluationResultParameter);
      SigmaParameter = cloner.Clone(original.SigmaParameter);
      PhiParameter = cloner.Clone(original.PhiParameter);
      MinPenaltyFactorParameter = cloner.Clone(original.MinPenaltyFactorParameter);
      MaxPenaltyFactorParameter = cloner.Clone(original.MaxPenaltyFactorParameter);
      CurrentTardinessPenaltyResult = cloner.Clone(original.CurrentTardinessPenaltyResult);
    }
    public TimeWindowRelaxationVRPAnalyzer()
      : base() {
      Parameters.Add(ProblemInstanceParameter = new LookupParameter<IVRPProblemInstance>("ProblemInstance", "The problem instance."));
      Parameters.Add(VRPToursParameter = new ScopeTreeLookupParameter<IVRPEncodedSolution>("VRPTours", "The VRP tours which should be evaluated."));
      Parameters.Add(EvaluationResultParameter = new ScopeTreeLookupParameter<CVRPTWEvaluation>("EvaluationResult", "The evaluations of the VRP solutions which should be analyzed."));

      Parameters.Add(SigmaParameter = new ValueParameter<DoubleValue>("Sigma", "The sigma applied to the penalty factor.", new DoubleValue(0.5)));
      Parameters.Add(PhiParameter = new ValueParameter<DoubleValue>("Phi", "The phi applied to the penalty factor.", new DoubleValue(0.5)));
      Parameters.Add(MinPenaltyFactorParameter = new ValueParameter<DoubleValue>("MinPenaltyFactor", "The minimum penalty factor.", new DoubleValue(0.01)));
      Parameters.Add(MaxPenaltyFactorParameter = new ValueParameter<DoubleValue>("MaxPenaltyFactor", "The maximum penalty factor.", new DoubleValue(100000)));
      Parameters.Add(CurrentTardinessPenaltyResult = new ResultParameter<DoubleValue>("Current Tardiness Penalty", "The current penalty applied to failing the time window constraint.", new DoubleValue(double.NaN)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new TimeWindowRelaxationVRPAnalyzer(this, cloner);
    }

    public override IOperation Apply() {
      var vrptw = (ITimeWindowedProblemInstance)ProblemInstanceParameter.ActualValue;

      ItemArray<CVRPTWEvaluation> evaluations = EvaluationResultParameter.ActualValue;

      double sigma = SigmaParameter.Value.Value;
      double phi = PhiParameter.Value.Value;
      double minPenalty = MinPenaltyFactorParameter.Value.Value;
      double maxPenalty = MaxPenaltyFactorParameter.Value.Value;

      for (int j = 0; j < evaluations.Length; j++) {
        evaluations[j].Quality -= evaluations[j].Tardiness * vrptw.TardinessPenalty.Value;
      }

      int validCount = 0;
      for (int j = 0; j < evaluations.Length; j++) {
        if (evaluations[j].Tardiness == 0)
          validCount++;
      }

      double factor = 1.0 - ((double)validCount / (double)evaluations.Length);

      double min = vrptw.TardinessPenalty.Value / (1 + sigma);
      double max = vrptw.TardinessPenalty.Value * (1 + phi);

      vrptw.CurrentTardinessPenalty = new DoubleValue(min + (max - min) * factor);
      if (vrptw.CurrentTardinessPenalty.Value < minPenalty)
        vrptw.CurrentTardinessPenalty.Value = minPenalty;
      if (vrptw.CurrentTardinessPenalty.Value > maxPenalty)
        vrptw.CurrentTardinessPenalty.Value = maxPenalty;

      for (int j = 0; j < evaluations.Length; j++) {
        evaluations[j].Quality += evaluations[j].Tardiness * vrptw.CurrentTardinessPenalty.Value;
      }

      CurrentTardinessPenaltyResult.ActualValue = new DoubleValue(vrptw.CurrentTardinessPenalty.Value);

      return base.Apply();
    }
  }
}
