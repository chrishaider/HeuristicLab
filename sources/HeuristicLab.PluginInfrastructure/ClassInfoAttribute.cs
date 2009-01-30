#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2008 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.Text;

namespace HeuristicLab.PluginInfrastructure {
  /// <summary>
  /// This attribute can be used to specify meta data for classes. 
  /// For example to specify name, version and description of applications or plugins.
  /// </summary>
  [AttributeUsage(AttributeTargets.Class)]
  public class ClassInfoAttribute : System.Attribute {
    private string name;
    /// <summary>
    /// Gets or sets the name of the plugin to which the assembly belongs to.
    /// </summary>
    public string Name {
      get { return name; }
      set { name = value; }
    }

    private string version;
    /// <summary>
    /// Gets or sets the version of the plugin.
    /// </summary>
    public string Version {
      get { return version; }
      set { version = value; }
    }

    private string description;
    /// <summary>
    /// Gets or sets the description of the plugin.
    /// </summary>
    public string Description {
      get { return description; }
      set { description = value; }
    }

    private bool autoRestart;
    /// <summary>
    /// Gets or sets the boolean flag whether the plugin should be automatically restarted.
    /// </summary>
    public bool AutoRestart {
      get { return autoRestart; }
      set { autoRestart = value; }
    }

    /// <summary>
    /// Initializes a new instance of <see cref="ClassInfoAttribute"/>.
    /// </summary>
    public ClassInfoAttribute() {}
  }
}
