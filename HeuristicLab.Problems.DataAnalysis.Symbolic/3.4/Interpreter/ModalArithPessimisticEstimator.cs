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
using System.Collections.Generic;
using System.Linq;
using HEAL.Attic;
using HeuristicLab.Common;
using HeuristicLab.Core;
using HeuristicLab.Data;
using HeuristicLab.Encodings.SymbolicExpressionTreeEncoding;
using HeuristicLab.Parameters;

namespace HeuristicLab.Problems.DataAnalysis.Symbolic {
  [StorableType("F31A89F7-F949-43CA-A5CC-4E04030AF056")]
  [Item("Modal Arithmetic Bounds Estimator", "Interpreter for calculation of modal intervals of symbolic models.")]
  public sealed class ModalArithPessimisticEstimator : ParameterizedNamedItem, IPessimisticEstimator {

    #region Parameters

    private const string EvaluatedSolutionsParameterName = "EvaluatedSolutions";

    public IFixedValueParameter<IntValue> EvaluatedSolutionsParameter =>
      (IFixedValueParameter<IntValue>)Parameters[EvaluatedSolutionsParameterName];

    public int EvaluatedSolutions {
      get => EvaluatedSolutionsParameter.Value.Value;
      set => EvaluatedSolutionsParameter.Value.Value = value;
    }
    #endregion

    #region Constructors

    [StorableConstructor]
    private ModalArithPessimisticEstimator(StorableConstructorFlag _) : base(_) { }

    private ModalArithPessimisticEstimator(ModalArithPessimisticEstimator original, Cloner cloner) : base(original, cloner) { }

    public ModalArithPessimisticEstimator() : base("Modal Arithmetic Bounds Estimator",
      "Estimates the bounds of the model with modal arithmetic") {
      Parameters.Add(new FixedValueParameter<IntValue>(EvaluatedSolutionsParameterName, "A counter for the total number of solutions the estimator has evaluated", new IntValue(0)));
    }

    public override IDeepCloneable Clone(Cloner cloner) {
      return new ModalArithPessimisticEstimator(this, cloner);
    }

    #endregion

    #region Evaluation

    private static Instruction[] PrepareInterpreterState(
      ISymbolicExpressionTree tree,
      IDictionary<string, ModalInterval> variableRanges) {
      if (variableRanges == null)
        throw new ArgumentNullException("No variable ranges are present!", nameof(variableRanges));

      // Check if all variables used in the tree are present in the dataset
      foreach (var variable in tree.IterateNodesPrefix().OfType<VariableTreeNode>().Select(n => n.VariableName)
                                   .Distinct())
        if (!variableRanges.ContainsKey(variable))
          throw new InvalidOperationException($"No ranges for variable {variable} is present");

      var code = SymbolicExpressionTreeCompiler.Compile(tree, OpCodes.MapSymbolToOpCode);
      foreach (var instr in code.Where(i => i.opCode == OpCodes.Variable)) {
        var variableTreeNode = (VariableTreeNode)instr.dynamicNode;
        instr.data = variableRanges[variableTreeNode.VariableName];
      }

      return code;
    }

    // Use ref parameter, because the tree will be iterated through recursively from the left-side branch to the right side
    // Update instructionCounter, whenever Evaluate is called
    public static ModalInterval Evaluate(
      Instruction[] instructions, ref int instructionCounter,
      IDictionary<ISymbolicExpressionTreeNode, ModalInterval> nodeIntervals = null,
      IDictionary<string, ModalInterval> variableIntervals = null) {
      var currentInstr = instructions[instructionCounter];
      instructionCounter++;
      ModalInterval result;

      switch (currentInstr.opCode) {
        case OpCodes.Variable: {
            var variableTreeNode = (VariableTreeNode)currentInstr.dynamicNode;
            var weightInterval = new ModalInterval(variableTreeNode.Weight, variableTreeNode.Weight);

            ModalInterval variableInterval;
            if (variableIntervals != null && variableIntervals.ContainsKey(variableTreeNode.VariableName))
              variableInterval = variableIntervals[variableTreeNode.VariableName];
            else
              variableInterval = (ModalInterval)currentInstr.data;

            result = ModalInterval.Mul(variableInterval, weightInterval);
            break;
          }
        case OpCodes.Constant: // fall through
        case OpCodes.Number: {
            var numericTreeNode = (INumericTreeNode)currentInstr.dynamicNode;
            result = new ModalInterval(numericTreeNode.Value, numericTreeNode.Value);
            break;
          }
        case OpCodes.Add: {
            result = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            for (var i = 1; i < currentInstr.nArguments; i++) {
              var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
              result = ModalInterval.Add(result, argumentInterval);
            }

            break;
          }
        case OpCodes.Sub: {
            result = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            if (currentInstr.nArguments == 1)
              result = ModalInterval.Mul(new ModalInterval(-1, -1), result);

            for (var i = 1; i < currentInstr.nArguments; i++) {
              var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
              result = ModalInterval.Sub(result, argumentInterval);
            }

            break;
          }
        case OpCodes.Mul: {
            result = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            for (var i = 1; i < currentInstr.nArguments; i++) {
              var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
              result = ModalInterval.Mul(result, argumentInterval);
            }

            break;
          }
        case OpCodes.Div: {
            result = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            if (currentInstr.nArguments == 1)
              result = ModalInterval.DivTrue(new ModalInterval(1, 1), result);

            for (var i = 1; i < currentInstr.nArguments; i++) {
              var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
              result = ModalInterval.DivTrue(result, argumentInterval);
            }

            break;
          }
        case OpCodes.Sin: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Sin(argumentInterval);
            break;
          }
        case OpCodes.Cos: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Cos(argumentInterval);
            break;
          }
        case OpCodes.Tan: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Tan(argumentInterval);
            break;
          }
        case OpCodes.Tanh: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Tanh(argumentInterval);
            break;
          }
        case OpCodes.Log: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Log(argumentInterval);
            break;
          }
        case OpCodes.Exp: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Exp(argumentInterval);
            break;
          }
        case OpCodes.Square: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Sqr(argumentInterval);
            break;
          }
        case OpCodes.SquareRoot: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Sqrt(argumentInterval);
            break;
          }
        /*case OpCodes.Cube: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Cube(argumentInterval);
            break;
          }
        case OpCodes.CubeRoot: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.CubicRoot(argumentInterval);
            break;
          }*/
        case OpCodes.Power: {
            var a = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            var b = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            // support only integer powers
            if (b.LowerBound == b.UpperBound && Math.Truncate(b.LowerBound) == b.LowerBound) {
              result = ModalInterval.Pow(a, (int)b.LowerBound);
            } else {
              throw new NotSupportedException("Interval is only supported for integer powers");
            }
            break;
          }
        case OpCodes.Absolute: {
            var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            result = ModalInterval.Abs(argumentInterval);
            break;
          }
        case OpCodes.AnalyticQuotient: {
            result = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            for (var i = 1; i < currentInstr.nArguments; i++) {
              var argumentInterval = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
              result = ModalInterval.AQ(result, argumentInterval);
            }

            break;
          }
        case OpCodes.SubFunction: {
            result = Evaluate(instructions, ref instructionCounter, nodeIntervals, variableIntervals);
            break;
          }
        default:
          throw new NotSupportedException(
            $"The tree contains the unknown symbol {currentInstr.dynamicNode.Symbol}");
      }

      if (!(nodeIntervals == null || nodeIntervals.ContainsKey(currentInstr.dynamicNode)))
        nodeIntervals.Add(currentInstr.dynamicNode, result);

      return result;
    }

    #endregion

    private readonly object syncRoot = new object();

    public static ModalInterval InnerApproximation(ISymbolicExpressionTree tree,
      Dictionary<string, ModalInterval> variableRanges) {
      var kTree = GetKTree(tree, variableRanges);
      var variables = tree.IterateNodesPostfix().OfType<VariableTreeNode>().Select(x => x.VariableName).Distinct();

      kTree = variables.Aggregate(kTree, (current, variable) => PointDual(tree, current, variable, variableRanges));

      //Evaluate the final kTree
      var instr = PrepareInterpreterState(kTree, variableRanges);
      var instrCount = 0;
      var bound = Evaluate(instr, ref instrCount, variableIntervals: variableRanges);
      return bound;
    }

    public static ISymbolicExpressionTree PointDual(ISymbolicExpressionTree tree, ISymbolicExpressionTree kTree,
      string variable, Dictionary<string, ModalInterval> variablesRanges) {
      //Check if variable occurres more than once
      var occurrences = GetOccurrencesOfVariable(tree, variable);
      if (occurrences <= 1)
        return kTree;

      kTree = Dualizer(tree, kTree, variable, variablesRanges, out var fullyMonotonic);

      if (fullyMonotonic) return kTree;

      var mid = variablesRanges[variable].Mid;
      variablesRanges[variable] = new ModalInterval(mid);
      for (var i = 1; i <= occurrences; ++i) {
        variablesRanges[variable + "_" + i] = new ModalInterval(mid);
      }

      return kTree;
    }

    public static ModalInterval OuterApproximation(ISymbolicExpressionTree tree,
      Dictionary<string, ModalInterval> variableRanges) {
      var kTree = GetKTree(tree, variableRanges);
      var variables = tree.IterateNodesPostfix().OfType<VariableTreeNode>().Select(x => x.VariableName).Distinct();

      kTree = variables.Aggregate(kTree, (current, variable) => Dualizer(tree, current, variable, variableRanges, out var _));

      //Evaluate the final kTree
      var instr = PrepareInterpreterState(kTree, variableRanges);
      var instrCount = 0;
      var bound = Evaluate(instr, ref instrCount, variableIntervals: variableRanges);
      return bound;
    }

    public static ISymbolicExpressionTree Dualizer(ISymbolicExpressionTree tree, ISymbolicExpressionTree kTree, string variable, Dictionary<string, ModalInterval> variableRanges, out bool fullyMonotoic) {
      //Check if variable occurres more than once
      var occurrences = GetOccurrencesOfVariable(tree, variable);
      fullyMonotoic = false;
      if (occurrences <= 1)
        return kTree;

      //Check monotonicity wrt the given variable
      var derived = DerivativeCalculator.Derive(tree, variable);
      var instr = PrepareInterpreterState(derived, variableRanges);
      var instrCount = 0;
      var bound = Evaluate(instr, ref instrCount, variableIntervals: variableRanges);
      if (!bound.IsMonotonic())
        return kTree;
      else {
        var dxSign = bound.GetMonotonicSign();

        //Check each occurrence of the variable
        var signDic = new Dictionary<string, int>();
        for (var i = 1; i <= occurrences; ++i) {
          var deriveVariable = variable + "_" + i;
          derived = DerivativeCalculator.Derive(kTree, deriveVariable);
          instr = PrepareInterpreterState(derived, variableRanges);
          instrCount = 0;
          bound = Evaluate(instr, ref instrCount, variableIntervals: variableRanges);
          var sign = bound.GetMonotonicSign();
          if (sign == 0)
            return kTree;
          signDic.Add(deriveVariable, sign);
        }

        //The variable an all occurrences of the variable are monotonic
        //Change the variable ranges regarding the rule x1_i if the monotonicity has the same sign as df/x1
        // or dual(x1_i) if the monotonicity has the opposite sign of df/x1
        fullyMonotoic = true;
        foreach (var kvp in signDic.Where(kvp => kvp.Value != dxSign)) {
          variableRanges[kvp.Key] = variableRanges[kvp.Key].Dual;
        }

        return kTree;
      }
    }

    private static int GetOccurrencesOfVariable(ISymbolicExpressionTree tree, string variable) {
      return tree.IterateNodesPostfix().OfType<VariableTreeNode>().Count(n => n.VariableName == variable);
    }

    public static ISymbolicExpressionTree GetKTree(ISymbolicExpressionTree tree, Dictionary<string, ModalInterval> variableRanges) {
      var kTree = (ISymbolicExpressionTree)tree.Clone();
      var distinctVariables = tree.IterateNodesPostfix().OfType<VariableTreeNode>().Select(v => v.VariableName).Distinct().ToList();

      foreach (var variable in distinctVariables) {
        var i = 1;
        foreach (var node in kTree.IterateNodesPostfix().OfType<VariableTreeNode>().Where(x => x.VariableName == variable)) {
          var newNodeName = node.VariableName + "_" + i;
          node.VariableName = newNodeName;
          variableRanges.Add(newNodeName, variableRanges[variable]);
          i++;
        }
      }
      return kTree;
    }

    public void ClearState() {
      EvaluatedSolutions = 0;
    }

    public double GetConstraintViolation(ISymbolicExpressionTree tree, IntervalCollection variableRanges, ShapeConstraint constraint) {

      var modelBound = GetModelBound(tree, variableRanges);

      if (constraint.Interval.Contains(modelBound)) return 0.0;


      var error = 0.0;

      if (!constraint.Interval.Contains(modelBound.LowerBound)) {
        error += Math.Abs(modelBound.LowerBound - constraint.Interval.LowerBound);
      }

      if (!constraint.Interval.Contains(modelBound.UpperBound)) {
        error += Math.Abs(modelBound.UpperBound - constraint.Interval.UpperBound);
      }

      return error;
    }

    public Interval GetModelBound(ISymbolicExpressionTree tree, IntervalCollection variableRanges) {
      lock (syncRoot) {
        EvaluatedSolutions++;
      }

      var modalDict = variableRanges.GetReadonlyDictionary().ToDictionary(variable => variable.Key, variable => new ModalInterval(variable.Value.LowerBound, variable.Value.UpperBound));

      var result = OuterApproximation(tree, modalDict);

      return result.ToInterval();
    }

    public IDictionary<ISymbolicExpressionTreeNode, Interval> GetModelNodeBounds(ISymbolicExpressionTree tree, IntervalCollection variableRanges) {
      throw new NotImplementedException();
    }

    public void InitializeState() {
      EvaluatedSolutions = 0;
    }

    public bool IsCompatible(ISymbolicExpressionTree tree) {
      var containsUnknownSymbols = (
        from n in tree.Root.GetSubtree(0).IterateNodesPrefix()
        where
          !(n.Symbol is Variable) &&
          !(n.Symbol is Number) &&
          !(n.Symbol is Constant) &&
          !(n.Symbol is StartSymbol) &&
          !(n.Symbol is Addition) &&
          !(n.Symbol is Subtraction) &&
          !(n.Symbol is Multiplication) &&
          !(n.Symbol is Division) &&
          !(n.Symbol is Sine) &&
          !(n.Symbol is Cosine) &&
          !(n.Symbol is Tangent) &&
          !(n.Symbol is HyperbolicTangent) &&
          !(n.Symbol is Logarithm) &&
          !(n.Symbol is Exponential) &&
          !(n.Symbol is Square) &&
          !(n.Symbol is SquareRoot) &&
          !(n.Symbol is Cube) &&
          !(n.Symbol is CubeRoot) &&
          !(n.Symbol is Power) &&
          !(n.Symbol is Absolute) &&
          !(n.Symbol is AnalyticQuotient) &&
          !(n.Symbol is SubFunctionSymbol)
        select n).Any();
      return !containsUnknownSymbols;
    }
  }
}
