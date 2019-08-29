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

using System;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Operators;
using HeuristicLab.Parameters;

namespace HeuristicLab.Optimization {
  [Item("Multi-objective Evaluator", "Calls the Evaluate method of the problem definition and writes the return value into the scope.")]
  [StorableType("C5605ED8-0ED2-4C7B-97A1-E7EB68A4FDBF")]
  public sealed class MultiObjectiveEvaluator<TEncodedSolution> : InstrumentedOperator, IMultiObjectiveEvaluationOperator<TEncodedSolution>, IStochasticOperator
  where TEncodedSolution : class, IEncodedSolution {

    public ILookupParameter<IRandom> RandomParameter {
      get { return (ILookupParameter<IRandom>)Parameters["Random"]; }
    }

    public ILookupParameter<IEncoding<TEncodedSolution>> EncodingParameter {
      get { return (ILookupParameter<IEncoding<TEncodedSolution>>)Parameters["Encoding"]; }
    }

    public ILookupParameter<DoubleArray> QualitiesParameter {
      get { return (ILookupParameter<DoubleArray>)Parameters["Qualities"]; }
    }

    public Func<TEncodedSolution, IRandom, double[]> EvaluateFunc { get; set; }

    [StorableConstructor]
    private MultiObjectiveEvaluator(StorableConstructorFlag _) : base(_) { }
    private MultiObjectiveEvaluator(MultiObjectiveEvaluator<TEncodedSolution> original, Cloner cloner) : base(original, cloner) { }
    public MultiObjectiveEvaluator() {
      Parameters.Add(new LookupParameter<IRandom>("Random", "The random number generator to use."));
      Parameters.Add(new LookupParameter<IEncoding<TEncodedSolution>>("Encoding", "An item that holds the problem's encoding."));
      Parameters.Add(new LookupParameter<DoubleArray>("Qualities", "The qualities of the parameter vector."));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new MultiObjectiveEvaluator<TEncodedSolution>(this, cloner);
    }

    public override IOperation InstrumentedApply() {
      var random = RandomParameter.ActualValue;
      var encoding = EncodingParameter.ActualValue;
      var solution = ScopeUtil.GetEncodedSolution(ExecutionContext.Scope, encoding);
      QualitiesParameter.ActualValue = new DoubleArray(EvaluateFunc(solution, random));
      return base.InstrumentedApply();
    }
  }
}
