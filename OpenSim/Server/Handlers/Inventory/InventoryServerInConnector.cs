/*
 * Copyright (c) Contributors, https://hyperionvirtual.com/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the Hyperion Virtual Worlds Project nor the
 *       names of its contributors may be used to endorse or promote products
 *       derived from this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
 * EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
 * WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
 * DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
 * (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
 * LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
 * ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
 * SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Reflection;
using log4net;
using Nini.Config;
using Nwc.XmlRpc;
using OpenSim.Server.Base;
using OpenSim.Services.Interfaces;
using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Server.Handlers.Base;
using OpenMetaverse;

namespace OpenSim.Server.Handlers.Inventory
{
    public class InventoryServiceInConnector : ServiceConnector
    {
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private IInventoryService m_InventoryService;

        private bool m_doLookup = false;

        //private static readonly int INVENTORY_DEFAULT_SESSION_TIME = 30; // secs
        //private AuthedSessionCache m_session_cache = new AuthedSessionCache(INVENTORY_DEFAULT_SESSION_TIME);

        private string m_userserver_url;

        public InventoryServiceInConnector(IConfigSource config, IHttpServer server) :
                base(config, server)
        {
            IConfig serverConfig = config.Configs["InventoryService"];
            if (serverConfig == null)
                throw new Exception("No section 'InventoryService' in config file");

            string inventoryService = serverConfig.GetString("LocalServiceModule",
                    String.Empty);

            if (inventoryService == String.Empty)
                throw new Exception("No InventoryService in config file");

            Object[] args = new Object[] { config };
            m_InventoryService =
                    ServerUtils.LoadPlugin<IInventoryService>(inventoryService, args);

            m_userserver_url = serverConfig.GetString("UserServerURI", String.Empty);
            m_doLookup = serverConfig.GetBoolean("SessionAuthentication", false);

            AddHttpHandlers(server);
        }

        protected virtual void AddHttpHandlers(IHttpServer m_httpServer)
        {
            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<Guid, InventoryCollection>(
                    "POST", "/GetInventory/", GetUserInventory, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryFolderBase, bool>(
                    "POST", "/UpdateFolder/", m_InventoryService.UpdateFolder, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryFolderBase, bool>(
                    "POST", "/MoveFolder/", m_InventoryService.MoveFolder, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryFolderBase, bool>(
                    "POST", "/PurgeFolder/", m_InventoryService.PurgeFolder, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryItemBase, bool>(
                    "POST", "/DeleteItem/", m_InventoryService.DeleteItem, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryItemBase, InventoryItemBase>(
                    "POST", "/QueryItem/", m_InventoryService.QueryItem, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryFolderBase, InventoryFolderBase>(
                    "POST", "/QueryFolder/", m_InventoryService.QueryFolder, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseTrustedHandler<Guid, bool>(
                    "POST", "/CreateInventory/", CreateUsersInventory, CheckTrustSource));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryFolderBase, bool>(
                    "POST", "/NewFolder/", m_InventoryService.AddFolder, CheckAuthSession));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseTrustedHandler<InventoryFolderBase, bool>(
                    "POST", "/CreateFolder/", m_InventoryService.AddFolder, CheckTrustSource));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseSecureHandler<InventoryItemBase, bool>(
                    "POST", "/NewItem/", m_InventoryService.AddItem, CheckAuthSession));

            m_httpServer.AddStreamHandler(
             new RestDeserialiseTrustedHandler<InventoryItemBase, bool>(
                 "POST", "/AddNewItem/", m_InventoryService.AddItem, CheckTrustSource));

            m_httpServer.AddStreamHandler(
                new RestDeserialiseTrustedHandler<Guid, List<InventoryItemBase>>(
                    "POST", "/GetItems/", GetFolderItems, CheckTrustSource));

            // for persistent active gestures
            m_httpServer.AddStreamHandler(
                new RestDeserialiseTrustedHandler<Guid, List<InventoryItemBase>>
                    ("POST", "/ActiveGestures/", GetActiveGestures, CheckTrustSource));

            // WARNING: Root folders no longer just delivers the root and immediate child folders (e.g
            // system folders such as Objects, Textures), but it now returns the entire inventory skeleton.
            // It would have been better to rename this request, but complexities in the BaseHttpServer
            // (e.g. any http request not found is automatically treated as an xmlrpc request) make it easier
            // to do this for now.
            m_httpServer.AddStreamHandler(
                new RestDeserialiseTrustedHandler<Guid, List<InventoryFolderBase>>
                    ("POST", "/RootFolders/", GetInventorySkeleton, CheckTrustSource));
        }

        #region Wrappers for converting the Guid parameter

        public InventoryCollection GetUserInventory(Guid guid)
        {
            UUID userID = new UUID(guid);
            return m_InventoryService.GetUserInventory(userID);
        }

        public List<InventoryItemBase> GetFolderItems(Guid folderID)
        {
            List<InventoryItemBase> allItems = new List<InventoryItemBase>();

            // TODO: UUID.Zero is passed as the userID here, making the old assumption that the OpenSim
            // inventory server only has a single inventory database and not per-user inventory databases.
            // This could be changed but it requirs a bit of hackery to pass another parameter into this
            // callback
            List<InventoryItemBase> items = m_InventoryService.GetFolderItems(UUID.Zero, new UUID(folderID));

            if (items != null)
            {
                allItems.InsertRange(0, items);
            }
            return allItems;
        }

        public bool CreateUsersInventory(Guid rawUserID)
        {
            UUID userID = new UUID(rawUserID);


            return m_InventoryService.CreateUserInventory(userID);
        }

        public List<InventoryItemBase> GetActiveGestures(Guid rawUserID)
        {
            UUID userID = new UUID(rawUserID);

            return m_InventoryService.GetActiveGestures(userID);
        }

        public List<InventoryFolderBase> GetInventorySkeleton(Guid rawUserID)
        {
            UUID userID = new UUID(rawUserID);
            return m_InventoryService.GetInventorySkeleton(userID);
        }

        #endregion

        /// <summary>
        /// Check that the source of an inventory request is one that we trust.
        /// </summary>
        /// <param name="peer"></param>
        /// <returns></returns>
        public bool CheckTrustSource(IPEndPoint peer)
        {
            if (m_doLookup)
            {
                m_log.InfoFormat("[INVENTORY IN CONNECTOR]: Checking trusted source {0}", peer);
                UriBuilder ub = new UriBuilder(m_userserver_url);
                IPAddress[] uaddrs = Dns.GetHostAddresses(ub.Host);
                foreach (IPAddress uaddr in uaddrs)
                {
                    if (uaddr.Equals(peer.Address))
                    {
                        return true;
                    }
                }

                m_log.WarnFormat(
                    "[INVENTORY IN CONNECTOR]: Rejecting request since source {0} was not in the list of trusted sources",
                    peer);

                return false;
            }
            else
            {
                return true;
            }
        }

        /// <summary>
        /// Check that the source of an inventory request for a particular agent is a current session belonging to
        /// that agent.
        /// </summary>
        /// <param name="session_id"></param>
        /// <param name="avatar_id"></param>
        /// <returns></returns>
        public bool CheckAuthSession(string session_id, string avatar_id)
        {
            if (m_doLookup)
            {
                m_log.InfoFormat("[INVENTORY IN CONNECTOR]: checking authed session {0} {1}", session_id, avatar_id);

                //if (m_session_cache.getCachedSession(session_id, avatar_id) == null)
                //{
                    // cache miss, ask userserver
                    Hashtable requestData = new Hashtable();
                    requestData["avatar_uuid"] = avatar_id;
                    requestData["session_id"] = session_id;
                    ArrayList SendParams = new ArrayList();
                    SendParams.Add(requestData);
                    XmlRpcRequest UserReq = new XmlRpcRequest("check_auth_session", SendParams);
                    XmlRpcResponse UserResp = UserReq.Send(m_userserver_url, 3000);

                    Hashtable responseData = (Hashtable)UserResp.Value;
                    if (responseData.ContainsKey("auth_session") && responseData["auth_session"].ToString() == "TRUE")
                    {
                        m_log.Info("[INVENTORY IN CONNECTOR]: got authed session from userserver");
                        //// add to cache; the session time will be automatically renewed
                        //m_session_cache.Add(session_id, avatar_id);
                        return true;
                    }
                //}
                //else
                //{
                //    // cache hits
                //    m_log.Info("[GRID AGENT INVENTORY]: got authed session from cache");
                //    return true;
                //}

                    m_log.Warn("[INVENTORY IN CONNECTOR]: unknown session_id, request rejected");
                return false;
            }
            else
            {
                return true;
            }
        }

    }
}
