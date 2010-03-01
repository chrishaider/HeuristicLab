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
using System.Text;
using System.Linq;
using System.Windows.Forms;
using HeuristicLab.PluginInfrastructure;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Net;
using HeuristicLab.Hive.Server.Core;
using HeuristicLab.Hive.Contracts;
using HeuristicLab.Hive.Contracts.Interfaces;
using HeuristicLab.Hive.Server.Properties;
using Spring.ServiceModel;

namespace HeuristicLab.Hive.Server {
  [Application("Hive Server", "Server application for the distributed hive engine.", true)]
  public class HiveServerApplication : ApplicationBase {
    public const string STR_ClientCommunicator = "ClientCommunicator";
    public const string STR_ServerConsoleFacade = "ServerConsoleFacade";
    public const string STR_ExecutionEngineFacade = "ExecutionEngineFacade";

    private Dictionary<string, ServiceHost> runningServices = new Dictionary<string, ServiceHost>();
    private NetTcpBinding binding = (NetTcpBinding)WcfSettings.GetBinding();
    private NetTcpBinding streamedBinding = (NetTcpBinding)WcfSettings.GetStreamedBinding();

    private enum Services {
      ClientCommunicator,
      ServerConsoleFacade,
      ExecutionEngineFacade,
      All
    }

    private bool AddMexEndpoint(ServiceHost serviceHost) {
      if (serviceHost != null) {
        ServiceMetadataBehavior behavior = new ServiceMetadataBehavior();
        serviceHost.Description.Behaviors.Add(behavior);

        return serviceHost.AddServiceEndpoint(
          typeof(IMetadataExchange),
          MetadataExchangeBindings.CreateMexTcpBinding(),
          "mex") != null;
      } else
        return false;
    }

    private Uri StartService(Services svc, IPAddress ipAddress, int port) {
      string curServiceHost = "";
      Uri uriTcp;
      /*IEnumerable<IClientFacade> clientCommunicatorInstances = ApplicationManager.Manager.GetInstances<IClientFacade>();
      IEnumerable<IServerConsoleFacade> serverConsoleInstances = ApplicationManager.Manager.GetInstances<IServerConsoleFacade>();
      IEnumerable<IExecutionEngineFacade> executionEngineInstances = ApplicationManager.Manager.GetInstances<IExecutionEngineFacade>();*/
      SpringServiceHost serviceHost = null;
      switch (svc) {
        case Services.ClientCommunicator:
        //  if (clientCommunicatorInstances.Count() > 0) {
            uriTcp = new Uri("net.tcp://" + ipAddress + ":" + port + "/HiveServer/");
            //serviceHost = new ServiceHost(clientCommunicatorInstances.First().GetType(), uriTcp);
            //serviceHost = new ServiceHost(typeof(ClientFacade), uriTcp);
            serviceHost = new SpringServiceHost("clientFacade", uriTcp);
            serviceHost.AddServiceEndpoint(typeof(IClientFacade), streamedBinding, STR_ClientCommunicator);
            
            ServiceDebugBehavior sdb = serviceHost.Description.Behaviors.Find<ServiceDebugBehavior>();
            if (sdb == null) {
              sdb = new ServiceDebugBehavior();
              serviceHost.Description.Behaviors.Add(sdb);
            }
            sdb.IncludeExceptionDetailInFaults = true;

            

            curServiceHost = STR_ClientCommunicator;
        //  }
          break;
        case Services.ServerConsoleFacade:
        //  if (serverConsoleInstances.Count() > 0) {
            uriTcp = new Uri("net.tcp://" + ipAddress + ":" + port + "/HiveServerConsole/");
            //serviceHost = new ServiceHost(serverConsoleInstances.First().GetType(), uriTcp);
            serviceHost = new SpringServiceHost("serverConsoleFacade", uriTcp);
            serviceHost.AddServiceEndpoint(typeof(IServerConsoleFacade), binding, STR_ServerConsoleFacade);
            curServiceHost = STR_ServerConsoleFacade;
        //  }
          break;
        case Services.ExecutionEngineFacade:
        //  if (executionEngineInstances.Count() > 0) {
            uriTcp = new Uri("net.tcp://" + ipAddress + ":" + port + "/ExecutionEngine/");
            //serviceHost = new ServiceHost(executionEngineInstances.First().GetType(), uriTcp);
            serviceHost = new SpringServiceHost("executionEngineFacade", uriTcp);
            serviceHost.AddServiceEndpoint(typeof(IExecutionEngineFacade), streamedBinding, STR_ExecutionEngineFacade);
            curServiceHost = STR_ExecutionEngineFacade;
        //  }
          break;
        case Services.All:
          throw new InvalidOperationException("Not supported!");
        default:
          return null;
      }
      if (!String.IsNullOrEmpty(curServiceHost)) {
        AddMexEndpoint(serviceHost);
        //WcfSettings.SetServiceCertificate(serviceHost);
        serviceHost.Open();
        runningServices.Add(curServiceHost, serviceHost);
        return serviceHost.BaseAddresses[0];
      } else
        return null;
    }

    private void StopService(Services svc) {
      ServiceHost svcHost = null;
      switch (svc) {
        case Services.ClientCommunicator:
          runningServices.TryGetValue(STR_ClientCommunicator, out svcHost);
          break;
        case Services.ServerConsoleFacade:
          runningServices.TryGetValue(STR_ServerConsoleFacade, out svcHost);
          break;
        case Services.ExecutionEngineFacade:
          runningServices.TryGetValue(STR_ExecutionEngineFacade, out svcHost);
          break;
        case Services.All:
          foreach (KeyValuePair<string, ServiceHost> item in runningServices)
            item.Value.Close();
          return;
        default:
          throw new InvalidOperationException("Not supported!");
      }
      svcHost.Close();
    }

    public override void Run() {
      IPAddress[] addresses = Dns.GetHostAddresses(Dns.GetHostName());
      int index = 0;
      if (System.Environment.OSVersion.Version.Major >= 6) {
        for (index = addresses.Length - 1; index >= 0; index--)
          if (addresses[index].AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
            break;
      }

      //Start services and record their base address
      Dictionary<string, Uri> baseAddrDict = new Dictionary<string, Uri>();
      baseAddrDict.Add(STR_ClientCommunicator,
        StartService(Services.ClientCommunicator, addresses[index], WcfSettings.DEFAULTPORT));
      baseAddrDict.Add(STR_ServerConsoleFacade,
        StartService(Services.ServerConsoleFacade, addresses[index], WcfSettings.DEFAULTPORT));
      baseAddrDict.Add(STR_ExecutionEngineFacade,
        StartService(Services.ExecutionEngineFacade, addresses[index], WcfSettings.DEFAULTPORT));

      IEnumerable<ILifecycleManager> lifecycleManagers = ApplicationManager.Manager.GetInstances<ILifecycleManager>();
      if (lifecycleManagers.Count() > 0) {
        ILifecycleManager lifecycleManager =
          lifecycleManagers.First();

        lifecycleManager.Init();
        Form mainForm = new MainForm(baseAddrDict);
        Application.Run(mainForm);

        lifecycleManager.Shutdown();
      }
      StopService(Services.All);
    }
  }
}
