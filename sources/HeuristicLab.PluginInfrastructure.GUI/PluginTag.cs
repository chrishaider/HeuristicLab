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

namespace HeuristicLab.PluginInfrastructure.GUI {

  [Flags]
  enum PluginState { 
    Installed = 1,
    Available = 2,
    Upgradeable = 4,
    Disabled = 8,
  };

  class PluginTag {
    private PluginInfo plugin;
    public PluginInfo Plugin {
      get { return plugin; }
    }

    private PluginDescription pluginDescription;
    internal PluginDescription PluginDescription {
      get { return pluginDescription; }
      set { 
        pluginDescription = value;
        pluginDependencies = GeneratePluginDependencies(pluginDescription);
        pluginDetails = GeneratePluginDetails(pluginDescription);
      }
    }

    private PluginDescription upgradePluginDescription;
    public PluginDescription UpgradePluginDescription {
      get { return upgradePluginDescription; }
      set {
        upgradePluginDescription = value;
        pluginDependencies = GeneratePluginDependencies(upgradePluginDescription);
        pluginDetails = GenerateUpgradeDetails(upgradePluginDescription);
        state = PluginState.Upgradeable;
      }
    }

    private string pluginName;
    public string PluginName {
      get { return pluginName; }
      set { pluginName = value; }
    }

    private Version pluginVersion;
    public Version PluginVersion {
      get { return pluginVersion; }
      set { pluginVersion = value; }
    }
    private DateTime pluginBuildDate;
    public DateTime PluginBuildDate {
      get { return pluginBuildDate; }
      set { pluginBuildDate = value; }
    }

    private string pluginDetails;
    private List<string> pluginDependencies;
    private List<PluginTag> allTags;

    private PluginState state;
    public PluginState State {
      get { return state; }
      set { state = value; }
    }

    private string message;
    public string Message {
      get { return message; }
    }

    public List<PluginTag> hull = new List<PluginTag>();

    public PluginTag(List<PluginTag> allTags, PluginInfo plugin, PluginState state) {
      this.plugin = plugin;
      this.state = state;
      this.allTags = allTags;
      this.pluginName = plugin.Name;
      this.pluginVersion = plugin.Version;
      this.pluginBuildDate = plugin.BuildDate;
      this.message = plugin.Message;
      pluginDetails = GeneratePluginDetails(plugin);
      pluginDependencies = GeneratePluginDependencies(plugin);
    }

    public PluginTag(List<PluginTag> allTags, PluginDescription plugin, PluginState state) {
      this.pluginDescription = plugin;
      this.state = state;
      this.allTags = allTags;
      this.pluginName = plugin.Name;
      this.pluginVersion = plugin.Version;
      this.pluginBuildDate = plugin.BuildDate;
      pluginDetails = GeneratePluginDetails(plugin);
      pluginDependencies = GeneratePluginDependencies(plugin);
    }



    private string GeneratePluginDetails(PluginInfo plugin) {
      string filenames = "";
      foreach (string filename in plugin.Files) {
        filenames += filename + "\n";
      }
      string dependencies = "";
      foreach (PluginInfo dependency in plugin.Dependencies) {
        dependencies += dependency.Name + " (" + dependency.Version + ")\n";
      }
      string dependents = "";
      PluginManager.Manager.GetDependentPlugins(plugin).ForEach(delegate(PluginInfo dependentPlugin) {
        dependents += dependentPlugin.Name + " (" + dependentPlugin.Version + ")\n";
      });
      return "Plugin: " + plugin.Name + "\n" +
      "Version: " + plugin.Version + "\n" +
      "Build: " +plugin.BuildDate + "\n\n" +
      (dependencies.Length != 0 ? "Requires: \n" + dependencies + "\n" : "") +
      (dependents.Length != 0 ? "Used by:\n" + dependents + "\n" : "") +
      (filenames.Length != 0 ? "Files:\n" + filenames + "\n" : "") + message;
      ;
    }

    private string GeneratePluginDetails(PluginDescription plugin) {
      string dependencies = "";
      foreach (string dependency in plugin.Dependencies) {
        dependencies += dependency + "\n";
      }
      return "plugin: " + plugin.Name + "\n" +
      "Version: " + plugin.Version + "\n" +
      "Build: "+plugin.BuildDate +"\n" +
      "Installed from: " + plugin.Source + "\n" +
      "Requires: \n" + dependencies;
    }

    private string GenerateUpgradeDetails(PluginDescription upgrade) {
      string dependencies = "";
      foreach(string dependency in upgrade.Dependencies) {
        dependencies += dependency + "\n";
      }
      return "plugin: " + upgrade.Name + "\n" +
      "Current version: " + plugin.Version + " ("+plugin.BuildDate+") will be upgraded to new version: " + upgrade.Version + " ("+upgrade.BuildDate+")\n"+
      "Upgraded from: " + upgrade.Source + "\n" +
      "Requires: \n" + dependencies;
    }

    private List<string> GeneratePluginDependencies(PluginInfo plugin) {
      List<string> dependencies = new List<string>();
      plugin.Dependencies.ForEach(delegate(PluginInfo dependency) {
        dependencies.Add(dependency.Name);
      });
      return dependencies;
    }

    private List<string> GeneratePluginDependencies(PluginDescription plugin) {
      return new List<string>(plugin.Dependencies);
    }

    internal string GetPluginDetails() {
      return pluginDetails;
    }

    internal List<PluginTag> GetDependentTags() {
      List<PluginTag> dependentTags = new List<PluginTag>();
      foreach(PluginTag tag in allTags) {
        if (tag.pluginDependencies.Contains(pluginName)) {
          if (!dependentTags.Contains(tag)) {
            dependentTags.Add(tag);

            tag.GetDependentTags().ForEach(delegate(PluginTag dependentTag) {
              if (!dependentTags.Contains(dependentTag)) {
                dependentTags.Add(dependentTag);
              }
            });
          }
        }
      }
      return dependentTags;
    }

    internal List<PluginTag> GetDependencyTags() {
      List<PluginTag> dependencyTags = new List<PluginTag>();
      foreach(PluginTag tag in allTags) {
        if (pluginDependencies.Contains(tag.pluginName)) {
          if (!dependencyTags.Contains(tag)) {
            dependencyTags.Add(tag);

            tag.GetDependencyTags().ForEach(delegate(PluginTag dependencyTag) {
              if (!dependencyTags.Contains(dependencyTag)) {
                dependencyTags.Add(dependencyTag);
              }
            });
          }
        }
      };
      return dependencyTags;
    }

    internal bool UpgradeAvailable() {
      return UpgradePluginDescription != null;
    }

    public override bool Equals(object obj) {
      if (obj == null || !(obj is PluginTag)) return false;
      PluginTag other = (PluginTag)obj;
      return other.pluginName == pluginName && other.pluginVersion == pluginVersion;
    }

    public override int GetHashCode() {
      if(pluginVersion != null) {
        return pluginName.GetHashCode() + pluginVersion.GetHashCode();
      } else return pluginName.GetHashCode();
    }
  }
}
