﻿using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace HeuristicLab.JsonInterface {
  public class UnsupportedJsonItem : JsonItem {
    public override string Name {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override string Description {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    public override string Path {
      get => throw new NotSupportedException();
    }

    [JsonIgnore]
    public override IEnumerable<IJsonItem> Children {
      get => throw new NotSupportedException();
      protected set => throw new NotSupportedException();
    }

    [JsonIgnore]
    public override IJsonItem Parent {
      get => throw new NotSupportedException();
      set => throw new NotSupportedException();
    }

    protected override ValidationResult Validate() => ValidationResult.Successful();
  }
}
