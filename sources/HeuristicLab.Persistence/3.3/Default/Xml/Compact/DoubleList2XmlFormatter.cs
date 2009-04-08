﻿using System.Collections;
using System.Collections.Generic;
using System;
using HeuristicLab.Persistence.Core;
using System.Globalization;

namespace HeuristicLab.Persistence.Default.Xml.Compact {

  [EmptyStorableClass]
  public class DoubleList2XmlFormatter : NumberEnumeration2XmlFormatterBase {

    public override Type Type {
      get {
        return typeof(List<double>);
      }
    }

    protected override void Add(IEnumerable enumeration, object o) {
      ((List<double>)enumeration).Add((int)o);
    }

    protected override object Instantiate() {
      return new List<double>();
    }

    protected override string FormatValue(object o) {
      return ((double)o).ToString("r", CultureInfo.InvariantCulture);
    }

    protected override object ParseValue(string o) {
      return double.Parse(o);
    }

  }
}