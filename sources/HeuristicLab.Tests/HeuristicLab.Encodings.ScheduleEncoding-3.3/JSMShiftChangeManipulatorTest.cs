﻿#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2013 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using HeuristicLab.Encodings.ScheduleEncoding.JobSequenceMatrix;
using HeuristicLab.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace HeuristicLab.Encodings.ScheduleEncoding_33.Tests {


  /// <summary>
  ///This is a test class for JSMShiftChangeManipulatorTest and is intended
  ///to contain all JSMShiftChangeManipulatorTest Unit Tests
  ///</summary>
  [TestClass()]
  public class JSMShiftChangeManipulatorTest {


    private TestContext testContextInstance;

    /// <summary>
    ///Gets or sets the test context which provides
    ///information about and functionality for the current test run.
    ///</summary>
    public TestContext TestContext {
      get {
        return testContextInstance;
      }
      set {
        testContextInstance = value;
      }
    }

    #region Additional test attributes
    // 
    //You can use the following additional attributes as you write your tests:
    //
    //Use ClassInitialize to run code before running the first test in the class
    //[ClassInitialize()]
    //public static void MyClassInitialize(TestContext testContext)
    //{
    //}
    //
    //Use ClassCleanup to run code after all tests in a class have run
    //[ClassCleanup()]
    //public static void MyClassCleanup()
    //{
    //}
    //
    //Use TestInitialize to run code before running each test
    //[TestInitialize()]
    //public void MyTestInitialize()
    //{
    //}
    //
    //Use TestCleanup to run code after each test has run
    //[TestCleanup()]
    //public void MyTestCleanup()
    //{
    //}
    //
    #endregion


    /// <summary>
    ///A test for Apply
    ///</summary>
    [TestMethod()]
    public void ApplyTest() {
      IRandom random = new TestRandom(new int[] { 2, 1, 1, 2, 1, 2, 1, 2, 1, 2, 1, 2 }, null);
      JSMEncoding individual = TestUtils.CreateTestJSM1();
      JSMShiftChangeManipulator.Apply(random, individual);
      JSMEncoding expected = new JSMEncoding();
      ItemList<Permutation> jsm = new ItemList<Permutation>();
      for (int i = 0; i < 3; i++) {
        jsm.Add(new Permutation(PermutationTypes.Absolute, new int[] { 0, 1, 3, 2, 4, 5 }));
        jsm.Add(new Permutation(PermutationTypes.Absolute, new int[] { 0, 1, 3, 4, 2, 5 }));
      }
      expected.JobSequenceMatrix = jsm;

      Assert.IsTrue(individual.Equals(expected));
    }
  }
}