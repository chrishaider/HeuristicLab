using System;
using System.Collections.Generic;
using System.Linq;
using HeuristicLab.Common;
using HeuristicLab.Random;

namespace HeuristicLab.Problems.Instances.DataAnalysis {
  public class Feynman98 : FeynmanDescriptor {
    private readonly int testSamples;
    private readonly int trainingSamples;

    public Feynman98() : this((int) DateTime.Now.Ticks, 10000, 10000, null) { }

    public Feynman98(int seed) {
      Seed            = seed;
      trainingSamples = 10000;
      testSamples     = 10000;
      noiseRatio      = null;
    }

    public Feynman98(int seed, int trainingSamples, int testSamples, double? noiseRatio) {
      Seed                 = seed;
      this.trainingSamples = trainingSamples;
      this.testSamples     = testSamples;
      this.noiseRatio      = noiseRatio;
    }

    public override string Name {
      get {
        return string.Format("III.17.37 beta*(1+alpha*cos(theta)) | {0} samples | {1}", trainingSamples,
          noiseRatio == null ? "no noise" : string.Format(System.Globalization.CultureInfo.InvariantCulture, "noise={0:g}",noiseRatio));
      }
    }

    protected override string TargetVariable { get { return noiseRatio == null ? "f" : "f_noise"; } }

    protected override string[] VariableNames {
      get { return new[] {"beta", "alpha", "theta", noiseRatio == null ? "f" : "f_noise"}; }
    }

    protected override string[] AllowedInputVariables { get { return new[] {"beta", "alpha", "theta"}; } }

    public int Seed { get; private set; }

    protected override int TrainingPartitionStart { get { return 0; } }
    protected override int TrainingPartitionEnd { get { return trainingSamples; } }
    protected override int TestPartitionStart { get { return trainingSamples; } }
    protected override int TestPartitionEnd { get { return trainingSamples + testSamples; } }

    protected override List<List<double>> GenerateValues() {
      var rand = new MersenneTwister((uint) Seed);

      var data  = new List<List<double>>();
      var beta  = ValueGenerator.GenerateUniformDistributedValues(rand.Next(), TestPartitionEnd, 1, 5).ToList();
      var alpha = ValueGenerator.GenerateUniformDistributedValues(rand.Next(), TestPartitionEnd, 1, 5).ToList();
      var theta = ValueGenerator.GenerateUniformDistributedValues(rand.Next(), TestPartitionEnd, 1, 5).ToList();

      var f = new List<double>();

      data.Add(beta);
      data.Add(alpha);
      data.Add(theta);
      data.Add(f);

      for (var i = 0; i < beta.Count; i++) {
        var res = beta[i] * (1 + alpha[i] * Math.Cos(theta[i]));
        f.Add(res);
      }

      if (noiseRatio != null) {
        var f_noise     = new List<double>();
        var sigma_noise = (double) noiseRatio * f.StandardDeviationPop();
        f_noise.AddRange(f.Select(md => md + NormalDistributedRandom.NextDouble(rand, 0, sigma_noise)));
        data.Remove(f);
        data.Add(f_noise);
      }

      return data;
    }
  }
}