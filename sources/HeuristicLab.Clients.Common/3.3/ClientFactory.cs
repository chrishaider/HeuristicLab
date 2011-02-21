#region License Information
/* HeuristicLab
 * Copyright (C) 2002-2011 Heuristic and Evolutionary Algorithms Laboratory (HEAL)
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
using System.ServiceModel;
using System.ServiceModel.Description;
using HeuristicLab.Clients.Common.Properties;
using HeuristicLab.Common;

namespace HeuristicLab.Clients.Common {
  public static class ClientFactory {
    public static T CreateClient<T, I>()
      where T : ClientBase<I>, I
      where I : class {
      return CreateClient<T, I>(null, null);
    }
    public static T CreateClient<T, I>(string endpointConfigurationName)
      where T : ClientBase<I>, I
      where I : class {
      return CreateClient<T, I>(endpointConfigurationName, null);
    }
    public static T CreateClient<T, I>(string endpointConfigurationName, string remoteAddress)
      where T : ClientBase<I>, I
      where I : class {
      return CreateClient<T, I>(endpointConfigurationName, remoteAddress, Settings.Default.UserName, Settings.Default.Password);
    }
    public static T CreateClient<T, I>(string endpointConfigurationName, string remoteAddress, string userName, string password)
      where T : ClientBase<I>, I
      where I : class {
      T client;
      if (string.IsNullOrEmpty(endpointConfigurationName)) {
        client = Activator.CreateInstance<T>();
      } else {
        client = (T)Activator.CreateInstance(typeof(T), endpointConfigurationName);
      }

      if (!string.IsNullOrEmpty(remoteAddress)) {
        SetEndpointAddress(client.Endpoint, remoteAddress);
      }

      client.ClientCredentials.UserName.UserName = userName;
      client.ClientCredentials.UserName.Password = password;
      client.ClientCredentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
      return client;
    }

    public static Disposable<ChannelFactory<I>> CreateChannelFactory<I>(string endpointConfigurationName) where I : class {
      return CreateChannelFactory<I>(endpointConfigurationName, null);
    }
    public static Disposable<ChannelFactory<I>> CreateChannelFactory<I>(string endpointConfigurationName, string remoteAddress) where I : class {
      return CreateChannelFactory<I>(endpointConfigurationName, remoteAddress, Settings.Default.UserName, Settings.Default.Password);
    }
    public static Disposable<ChannelFactory<I>> CreateChannelFactory<I>(string endpointConfigurationName, string remoteAddress, string userName, string password) where I : class {
      ChannelFactory<I> factory = GetChannelFactory<I>(endpointConfigurationName, userName, password);

      if (!string.IsNullOrEmpty(remoteAddress)) {
        SetEndpointAddress(factory.Endpoint, remoteAddress);
      }
      var disposable = new Disposable<ChannelFactory<I>>(factory);
      disposable.OnDisposing += new EventHandler<EventArgs<object>>(disposable_OnDisposing);
      return disposable;
    }

    private static void disposable_OnDisposing(object sender, EventArgs<object> e) {
      DisposeCommunicationObject((ICommunicationObject)e.Value);
      ((Disposable)sender).OnDisposing -= new EventHandler<EventArgs<object>>(disposable_OnDisposing);
    }

    private static ChannelFactory<I> GetChannelFactory<I>(string endpointConfigurationName, string userName, string password) where I : class {
      var channelFactory = new ChannelFactory<I>(endpointConfigurationName);
      channelFactory.Credentials.UserName.UserName = userName;
      channelFactory.Credentials.UserName.Password = password;
      channelFactory.Credentials.ServiceCertificate.Authentication.CertificateValidationMode = System.ServiceModel.Security.X509CertificateValidationMode.None;
      return channelFactory;
    }

    public static void DisposeCommunicationObject(ICommunicationObject obj) {
      if (obj != null) {
        if (obj.State != CommunicationState.Faulted && obj.State != CommunicationState.Closed) {
          try { obj.Close(); }
          catch { obj.Abort(); }
        } else {
          obj.Abort();
        }
      }
    }

    /// <summary>
    /// This method changes the endpoint-address while preserving the identity-certificate defined in the config file
    /// </summary>
    private static void SetEndpointAddress(ServiceEndpoint endpoint, string remoteAddress) {
      EndpointAddressBuilder endpointAddressbuilder = new EndpointAddressBuilder(endpoint.Address);
      UriBuilder uriBuilder = new UriBuilder(endpointAddressbuilder.Uri);
      uriBuilder.Host = remoteAddress;
      endpointAddressbuilder.Uri = uriBuilder.Uri;
      endpoint.Address = endpointAddressbuilder.ToEndpointAddress();
    }
  }
}
