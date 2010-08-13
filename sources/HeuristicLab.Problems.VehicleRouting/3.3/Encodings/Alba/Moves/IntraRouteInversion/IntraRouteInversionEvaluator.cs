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

using HeuristicLab.Core;
using HeuristicLab.Encodings.PermutationEncoding;
using HeuristicLab.Parameters;
using HeuristicLab.Persistence.Default.CompositeSerializers.Storable;
using HeuristicLab.Problems.VehicleRouting.Encodings.Alba;
using HeuristicLab.Data;

namespace HeuristicLab.Problems.VehicleRouting {
  [Item("IntraRouteInversionMoveEvaluator", "Evaluates a intra route inversion move for a VRP representation.")]
  [StorableClass]
  public sealed class IntraRouteInversionMoveEvaluator : VRPMoveEvaluator, IAlbaIntraRouteInversionMoveOperator {
    public ILookupParameter<IntraRouteInversionMove> IntraRouteInversionMoveParameter {
      get { return (ILookupParameter<IntraRouteInversionMove>)Parameters["IntraRouteInversionMove"]; }
    }

    [StorableConstructor]
    private IntraRouteInversionMoveEvaluator(bool deserializing) : base(deserializing) { }

    public IntraRouteInversionMoveEvaluator()
      : base() {
        Parameters.Add(new LookupParameter<IntraRouteInversionMove>("IntraRouteInversionMove", "The move to evaluate."));
    }

    public static TourEvaluation GetMoveQuality(AlbaEncoding individual, IntraRouteInversionMove move,
      DoubleArray dueTimeArray, DoubleArray serviceTimeArray, DoubleArray readyTimeArray,
      DoubleArray demandArray, DoubleValue capacity, DoubleMatrix coordinates,
      DoubleValue fleetUsageFactor, DoubleValue timeFactor, DoubleValue distanceFactor,
      DoubleValue overloadPenalty, DoubleValue tardinessPenalty,
      ILookupParameter<DoubleMatrix> distanceMatrix, Data.BoolValue useDistanceMatrix) {
      AlbaEncoding newSolution = individual.Clone() as AlbaEncoding;
      IntraRouteInversionMoveMaker.Apply(newSolution, move);

      return VRPEvaluator.Evaluate(
        newSolution, dueTimeArray, serviceTimeArray, readyTimeArray,
        demandArray, capacity, fleetUsageFactor, timeFactor, distanceFactor,
        overloadPenalty, tardinessPenalty, coordinates, distanceMatrix, useDistanceMatrix);
    }

    protected override TourEvaluation GetMoveQuality() {
      return GetMoveQuality(
        VRPToursParameter.ActualValue as AlbaEncoding, IntraRouteInversionMoveParameter.ActualValue,
        DueTimeParameter.ActualValue, ServiceTimeParameter.ActualValue, ReadyTimeParameter.ActualValue,
        DemandParameter.ActualValue, CapacityParameter.ActualValue, CoordinatesParameter.ActualValue,
        FleetUsageFactor.ActualValue, TimeFactor.ActualValue, DistanceFactor.ActualValue, OverloadPenalty.ActualValue,
        TardinessPenalty.ActualValue, DistanceMatrixParameter, UseDistanceMatrixParameter.ActualValue);
    }
  }
}
