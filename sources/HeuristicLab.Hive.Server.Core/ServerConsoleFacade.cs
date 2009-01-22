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
using HeuristicLab.Hive.Contracts.Interfaces;
using HeuristicLab.Hive.Contracts.BusinessObjects;
using HeuristicLab.Hive.Contracts;

namespace HeuristicLab.Hive.Server.Core {
  public class ServerConsoleFacade: IServerConsoleFacade {
    private IClientManager clientManager = 
      ServiceLocator.GetClientManager();

    private IJobManager jobManager = 
      ServiceLocator.GetJobManager();

    private IUserRoleManager userRoleManager = 
      ServiceLocator.GetUserRoleManager();

    private String loginName = null;

    #region IServerConsoleFacade Members

    public Response Login(string username, string password) {
      Response resp = new Response();

      loginName = username;

      resp.Success = true;
      resp.StatusMessage =
        ApplicationConstants.RESPONSE_SERVERCONSOLE_LOGIN_SUCCESS;

      return resp;
    }

    #endregion

    #region IClientManager Members

    public ResponseList<ClientInfo> GetAllClients() {
      return clientManager.GetAllClients();
    }

    public ResponseList<ClientGroup> GetAllClientGroups() {
      return clientManager.GetAllClientGroups();
    }

    public ResponseList<UpTimeStatistics> GetAllUpTimeStatistics() {
      return clientManager.GetAllUpTimeStatistics();
    }

    public Response AddClientGroup(ClientGroup clientGroup) {
      return clientManager.AddClientGroup(clientGroup);
    }

    public Response AddResourceToGroup(long clientGroupId, Resource resource) {
      return clientManager.AddResourceToGroup(clientGroupId, resource);
    }

    public Response DeleteResourceFromGroup(long clientGroupId, long resourceId) {
      return clientManager.DeleteResourceFromGroup(clientGroupId, resourceId);
    }

    #endregion

    #region IJobManager Members

    public ResponseList<HeuristicLab.Hive.Contracts.BusinessObjects.Job> GetAllJobs() {
      return jobManager.GetAllJobs();
    }
    public ResponseObject<Job> AddNewJob(Job job) {
      return jobManager.AddNewJob(job);
    }

    public ResponseObject<JobResult> GetLastJobResultOf(long jobId) {
      return jobManager.GetLastJobResultOf(jobId);
    }

    public Response RemoveJob(long jobId) {
      return jobManager.RemoveJob(jobId);
    }

    #endregion

    #region IUserRoleManager Members

    public ResponseList<HeuristicLab.Hive.Contracts.BusinessObjects.User> GetAllUsers() {
      return userRoleManager.GetAllUsers();
    }

    public ResponseObject<User> AddNewUser(User user) {
      return userRoleManager.AddNewUser(user);
    }

    public ResponseList<UserGroup> GetAllUserGroups() {
      return userRoleManager.GetAllUserGroups();
    }

    public Response RemoveUser(long userId) {
      return userRoleManager.RemoveUser(userId);
    }

    public ResponseObject<UserGroup> AddNewUserGroup(UserGroup userGroup) {
      return userRoleManager.AddNewUserGroup(userGroup);
    }

    public Response RemoveUserGroup(long groupId) {
      return userRoleManager.RemoveUserGroup(groupId);
    }

    public Response AddUserToGroup(long groupId, long userId) {
      return userRoleManager.AddUserToGroup(groupId, userId);
    }

    public Response AddUserGroupToGroup(long groupId, long groupToAddId) {
      return userRoleManager.AddUserGroupToGroup(groupId, groupToAddId);
    }

    public Response RemovePermissionOwnerFromGroup(long groupId, long userId) {
      return userRoleManager.RemovePermissionOwnerFromGroup(groupId, userId);
    }

    #endregion

  }
}
