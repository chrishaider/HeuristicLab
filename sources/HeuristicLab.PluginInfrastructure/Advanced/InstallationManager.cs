﻿#region License Information
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
using System.Linq;
using System.Text;
using HeuristicLab.PluginInfrastructure.Manager;
using System.IO;
using System.ComponentModel;
using HeuristicLab.PluginInfrastructure.UpdateLocationReference;
using System.Reflection;

namespace HeuristicLab.PluginInfrastructure.Advanced {
  internal class InstallationManager {

    internal event EventHandler<PluginInfrastructureCancelEventArgs> PreUpdatePlugin;
    internal event EventHandler<PluginInfrastructureCancelEventArgs> PreRemovePlugin;
    internal event EventHandler<PluginInfrastructureCancelEventArgs> PreInstallPlugin;

    internal event EventHandler<PluginInfrastructureEventArgs> PluginUpdated;
    internal event EventHandler<PluginInfrastructureEventArgs> PluginRemoved;
    internal event EventHandler<PluginInfrastructureEventArgs> PluginInstalled;


    public IEnumerable<PluginDescription> Plugins {
      get { return pluginManager.Plugins; }
    }

    private string pluginDir;
    private string updateLocationUrl;
    private PluginManager pluginManager;
    public InstallationManager(string pluginDir) {
      this.pluginDir = pluginDir;
      this.updateLocationUrl = "http://localhost:59253/UpdateLocation.svc";
      this.pluginManager = new PluginManager(pluginDir);
      this.pluginManager.DiscoverAndCheckPlugins();
    }

    public IEnumerable<string> Show(IEnumerable<string> pluginNames) {
      foreach (PluginDescription desc in GetPluginDescriptions(pluginNames)) {
        yield return GetInformation(desc);
      }
    }

    internal string GetInformation(string pluginName) {
      return GetInformation(GetPluginDescription(pluginName));
    }

    private string GetInformation(PluginDescription desc) {
      StringBuilder builder = new StringBuilder();
      builder.Append("Name: ").AppendLine(desc.Name);
      builder.Append("Version: ").AppendLine(desc.Version.ToString());
      builder.AppendLine("Description:").AppendLine(desc.Description);
      if (!string.IsNullOrEmpty(desc.ContactName)) {
        builder.Append("Contact: ").Append(desc.ContactName).Append(", ").AppendLine(desc.ContactEmail);
      }
      builder.AppendLine("This plugin is " + desc.PluginState.ToString().ToLowerInvariant() + ".");
      // builder.Append("Build date: ").AppendLine(desc.BuildDate.ToString()).AppendLine();
      builder.AppendLine("Files: ");
      foreach (var file in desc.Files) {
        builder.AppendLine(file.Type + " " + file.Name);
      }
      builder.AppendLine().AppendLine("Directly depends on:");
      if (desc.Dependencies.Count() == 0) builder.AppendLine("None");
      foreach (var dependency in desc.Dependencies) {
        builder.AppendLine(dependency.Name + " " + dependency.Version);
      }
      builder.AppendLine().AppendFormat("Plugins directly dependent on {0}:", desc.Name).AppendLine();
      var dependents = from x in pluginManager.Plugins
                       where x.Dependencies.Contains(desc)
                       select x;
      if (dependents.Count() == 0) builder.AppendLine("None");
      foreach (var dependent in dependents) {
        builder.AppendLine(dependent.Name + " " + dependent.Version);
      }
      builder.AppendLine();
      if (desc.PluginState == PluginState.Disabled) {
        builder.AppendLine(DetermineProblem(desc));
      }

      return builder.ToString();
    }

    private static string DetermineProblem(PluginDescription desc) {
      // either any file is missing
      StringBuilder builder = new StringBuilder();
      builder.AppendLine("Problem report:");
      builder.AppendLine(desc.LoadingErrorInformation);
      return builder.ToString();
    }

    private static IEnumerable<string> GetDeclaredDependencies(PluginDescription desc) {
      var plugin = ApplicationManager.GetInstances<IPlugin>(desc).Single();
      return plugin.GetType().GetCustomAttributes(typeof(PluginDependencyAttribute), false).Cast<PluginDependencyAttribute>().Select(x => x.Dependency);
    }

    private PluginDescription GetPluginDescription(string pluginName) {
      var exactMatch = from pluginDesc in pluginManager.Plugins
                       where string.Equals(pluginName, pluginDesc.Name, StringComparison.InvariantCultureIgnoreCase)
                       select pluginDesc;
      var inexactMatch = from pluginDesc in pluginManager.Plugins
                         where MatchPluginNameInexact(pluginName, pluginDesc.Name)
                         select pluginDesc;
      return exactMatch.Count() > 0 ? exactMatch.Single() : inexactMatch.First();
    }

    private IEnumerable<PluginDescription> GetPluginDescriptions(IEnumerable<string> pluginNames) {
      return from pluginName in pluginNames
             select GetPluginDescription(pluginName);
    }

    private static bool MatchPluginNameInexact(string similarName, string actualName) {
      return
        // Core-3.2 == HeuristicLab.Core-3.2
        actualName.Equals("HeuristicLab." + similarName, StringComparison.InvariantCultureIgnoreCase) ||
        // HeuristicLab.Core == HeuristicLab.Core-3.2 (this should be save because we checked for exact matches first)
        (Math.Abs(actualName.Length - similarName.Length) <= 4 && actualName.StartsWith(similarName, StringComparison.InvariantCultureIgnoreCase)) ||
        // Core == HeuristicLab.Core-3.2
        (Math.Abs(actualName.Length - similarName.Length) <= 17 && actualName.StartsWith("HeuristicLab." + similarName, StringComparison.InvariantCultureIgnoreCase));
    }

    public void Install(IEnumerable<string> pluginNames) {
      throw new NotImplementedException();
      //IEnumerable<PluginInformation> pluginsToInstall;
      //using (UpdateLocationClient updateLocation = new UpdateLocationClient()) {
      //  pluginsToInstall = from pluginName in pluginNames
      //                     from matchingPlugin in updateLocation.GetAvailablePluginsByName(pluginName)
      //                     select matchingPlugin;

      //  var args = new PluginInfrastructureCancelEventArgs("Installing", pluginsToInstall);
      //  OnPreInstall(args);
      //  foreach (var pluginInfo in pluginsToInstall) {
      //    var s = updateLocation.GetPluginFiles(pluginInfo);
      //    Console.WriteLine("Downloading: {0} {1} {2}", pluginInfo.Name, pluginInfo.Version, pluginInfo.BuildDate);
      //  }
      //}
      //OnInstalled(new PluginInfrastructureEventArgs("Installed", pluginsToInstall));
    }

    //private static PluginInformation GetMatchingPluginInformation(string pluginName, IEnumerable<PluginInformation> plugins) {
    //  var exactMatch = from pluginDesc in plugins
    //                   where string.Equals(pluginName, pluginDesc.Name, StringComparison.InvariantCultureIgnoreCase)
    //                   select pluginDesc;
    //  var inexactMatch = from pluginDesc in plugins
    //                     where MatchPluginNameInexact(pluginName, pluginDesc.Name)
    //                     select pluginDesc;
    //  return exactMatch.Count() > 0 ? exactMatch.Single() : inexactMatch.First();
    //}

    public void Remove(IEnumerable<string> pluginNames) {
      var fileNames = from pluginToDelete in PluginDescriptionIterator.IterateDependentsTopDown(GetPluginDescriptions(pluginNames), pluginManager.Plugins)
                      from file in pluginToDelete.Files
                      select Path.Combine(pluginDir, file.Name);
      var args = new PluginInfrastructureCancelEventArgs("Deleting", fileNames);
      OnPreDelete(args);
      if (!args.Cancel) {
        foreach (string fileName in fileNames) {
          Console.WriteLine("Deleting file " + fileName);
          // File.Delete(fileName);
        }

        OnDeleted(new PluginInfrastructureEventArgs("Deleted", fileNames));
      }
    }

    public void Update(IEnumerable<string> pluginNames) {
      var pluginDescriptions = from name in pluginNames
                               select GetPluginDescription(name);
      Dictionary<PluginInformation, string> matchingPlugins = new Dictionary<PluginInformation, string>();
      foreach (var updateLocation in HeuristicLab.PluginInfrastructure.Properties.Settings.Default.UpdateLocations) {
        using (UpdateLocationClient client = new UpdateLocationClient("", updateLocation)) {
          var updateLocationMatchingPlugins = from desc in pluginDescriptions
                                              from info in client.GetAvailablePluginsByName(desc.Name)
                                              select info;
          foreach (PluginInformation info in updateLocationMatchingPlugins) {
            // keep only the highest version and most recent build of any plugin
            var existingPlugin = matchingPlugins.Keys.FirstOrDefault(x => x.Name == info.Name);
            if (existingPlugin == null || existingPlugin.Version < info.Version || (existingPlugin.Version == info.Version && existingPlugin.BuildDate < info.BuildDate)) {
              matchingPlugins.Remove(existingPlugin);
              matchingPlugins.Add(info, updateLocation);
            }
          }
        }
      }
      PluginInfrastructureCancelEventArgs args = new PluginInfrastructureCancelEventArgs("Updating", matchingPlugins.Keys);
      OnPreUpdate(args);
      if (!args.Cancel) {
        var groupedInfos = matchingPlugins.GroupBy(x => x.Value);
        foreach (var group in groupedInfos) {
          using (UpdateLocationClient client = new UpdateLocationClient(group.Key)) {
            foreach (var info in group) {
              client.GetPluginFiles(info.Key);
            }
          }
        }
        OnUpdated(new PluginInfrastructureEventArgs("Updated", matchingPlugins.Keys));
      }
    }

    private void OnPreUpdate(PluginInfrastructureCancelEventArgs args) {
      if (PreUpdatePlugin != null) PreUpdatePlugin(this, args);
    }

    private void OnUpdated(PluginInfrastructureEventArgs args) {
      if (PluginUpdated != null) PluginUpdated(this, args);
    }

    private void OnPreDelete(PluginInfrastructureCancelEventArgs args) {
      if (PreRemovePlugin != null) PreRemovePlugin(this, args);
    }

    private void OnDeleted(PluginInfrastructureEventArgs args) {
      if (PluginRemoved != null) PluginRemoved(this, args);
    }

    private void OnPreInstall(PluginInfrastructureCancelEventArgs args) {
      if (PreInstallPlugin != null) PreInstallPlugin(this, args);
    }

    private void OnInstalled(PluginInfrastructureEventArgs args) {
      if (PluginInstalled != null) PluginInstalled(this, args);
    }
  }
}
