/*
 * Copyright (c) Contributors, http://opensimulator.org/
 * See CONTRIBUTORS.TXT for a full list of copyright holders.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are met:
 *     * Redistributions of source code must retain the above copyright
 *       notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above copyright
 *       notice, this list of conditions and the following disclaimer in the
 *       documentation and/or other materials provided with the distribution.
 *     * Neither the name of the OpenSimulator Project nor the
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

using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Nini.Config;
using OpenSim.Framework;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Services.Interfaces;
using OpenMetaverse;

namespace OpenSim.Services.Connectors
{
    public class InventoryServicesConnector : ISessionAuthInventoryService
    {
        private static readonly ILog m_log =
                LogManager.GetLogger(
                MethodBase.GetCurrentMethod().DeclaringType);

        private string m_ServerURI = String.Empty;

        private Dictionary<UUID, InventoryReceiptCallback> m_RequestingInventory = new Dictionary<UUID, InventoryReceiptCallback>();

        public InventoryServicesConnector()
        {
        }

        public InventoryServicesConnector(string serverURI)
        {
            m_ServerURI = serverURI.TrimEnd('/');
        }

        public InventoryServicesConnector(IConfigSource source)
        {
            Initialise(source);
        }

        public virtual void Initialise(IConfigSource source)
        {
            IConfig inventoryConfig = source.Configs["InventoryService"];
            if (inventoryConfig == null)
            {
                m_log.Error("[INVENTORY CONNECTOR]: InventoryService missing from OpenSim.ini");
                throw new Exception("Inventory connector init error");
            }

            string serviceURI = inventoryConfig.GetString("InventoryServerURI",
                    String.Empty);

            if (serviceURI == String.Empty)
            {
                m_log.Error("[INVENTORY CONNECTOR]: No Server URI named in section InventoryService");
                throw new Exception("Unable to proceed. Please make sure your ini files in config-include are updated according to .example's");
            }
            m_ServerURI = serviceURI.TrimEnd('/');
        }

        #region ISessionAuthInventoryService

        public string Host
        {
            get { return m_ServerURI; }
        }

        /// <summary>
        /// Caller must catch eventual Exceptions.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="sessionID"></param>
        /// <param name="callback"></param>
        public void GetUserInventory(string userIDStr, UUID sessionID, InventoryReceiptCallback callback)
        {
            UUID userID = UUID.Zero;
            if (UUID.TryParse(userIDStr, out userID))
            {
                lock (m_RequestingInventory)
                {
                    if (!m_RequestingInventory.ContainsKey(userID))
                        m_RequestingInventory.Add(userID, callback);
                    else
                    {
                        m_log.ErrorFormat("[INVENTORY CONNECTOR]: GetUserInventory - ignoring repeated request for user {0}", userID);
                        return;
                    }
                }

                m_log.InfoFormat(
                    "[INVENTORY CONNECTOR]: Requesting inventory from {0}/GetInventory/ for user {1}",
                    m_ServerURI, userID);

                RestSessionObjectPosterResponse<Guid, InventoryCollection> requester
                    = new RestSessionObjectPosterResponse<Guid, InventoryCollection>();
                requester.ResponseCallback = InventoryResponse;

                requester.BeginPostObject(m_ServerURI + "/GetInventory/", userID.Guid, sessionID.ToString(), userID.ToString());
            }
        }

        public bool AddFolder(string userID, InventoryFolderBase folder, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryFolderBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/NewFolder/", folder, sessionID.ToString(), folder.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Add new inventory folder operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public bool UpdateFolder(string userID, InventoryFolderBase folder, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryFolderBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/UpdateFolder/", folder, sessionID.ToString(), folder.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Update inventory folder operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public bool MoveFolder(string userID, InventoryFolderBase folder, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryFolderBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/MoveFolder/", folder, sessionID.ToString(), folder.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Move inventory folder operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public bool PurgeFolder(string userID, InventoryFolderBase folder, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryFolderBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/PurgeFolder/", folder, sessionID.ToString(), folder.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Move inventory folder operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public bool AddItem(string userID, InventoryItemBase item, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryItemBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/NewItem/", item, sessionID.ToString(), item.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Add new inventory item operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public bool UpdateItem(string userID, InventoryItemBase item, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryItemBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/NewItem/", item, sessionID.ToString(), item.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Update new inventory item operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public bool DeleteItem(string userID, InventoryItemBase item, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryItemBase, bool>.BeginPostObject(
                    "POST", m_ServerURI + "/DeleteItem/", item, sessionID.ToString(), item.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Delete inventory item operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return false;
        }

        public InventoryItemBase QueryItem(string userID, InventoryItemBase item, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryItemBase, InventoryItemBase>.BeginPostObject(
                    "POST", m_ServerURI + "/QueryItem/", item, sessionID.ToString(), item.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Query inventory item operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return null;
        }

        public InventoryFolderBase QueryFolder(string userID, InventoryFolderBase item, UUID sessionID)
        {
            try
            {
                return SynchronousRestSessionObjectPoster<InventoryFolderBase, InventoryFolderBase>.BeginPostObject(
                    "POST", m_ServerURI + "/QueryFolder/", item, sessionID.ToString(), item.Owner.ToString());
            }
            catch (Exception e)
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Query inventory item operation failed, {0} {1}",
                     e.Source, e.Message);
            }

            return null;
        }

        #endregion

        /// <summary>
        /// Callback used by the inventory server GetInventory request
        /// </summary>
        /// <param name="userID"></param>
        private void InventoryResponse(InventoryCollection response)
        {
            UUID userID = response.UserID;
            InventoryReceiptCallback callback = null;
            lock (m_RequestingInventory)
            {
                if (m_RequestingInventory.ContainsKey(userID))
                {
                    callback = m_RequestingInventory[userID];
                    m_RequestingInventory.Remove(userID);
                }
                else
                {
                    m_log.WarnFormat(
                        "[INVENTORY CONNECTOR]: " +
                        "Received inventory response for {0} for which we do not have a record of requesting!",
                        userID);
                    return;
                }
            }

            m_log.InfoFormat("[INVENTORY CONNECTOR]: " +
                             "Received inventory response for user {0} containing {1} folders and {2} items",
                             userID, response.Folders.Count, response.Items.Count);

            InventoryFolderImpl rootFolder = null;

            ICollection<InventoryFolderImpl> folders = new List<InventoryFolderImpl>();
            ICollection<InventoryItemBase> items = new List<InventoryItemBase>();

            foreach (InventoryFolderBase folder in response.Folders)
            {
                if (folder.ParentID == UUID.Zero)
                {
                    rootFolder = new InventoryFolderImpl(folder);
                    folders.Add(rootFolder);

                    break;
                }
            }

            if (rootFolder != null)
            {
                foreach (InventoryFolderBase folder in response.Folders)
                {
                    if (folder.ID != rootFolder.ID)
                    {
                        folders.Add(new InventoryFolderImpl(folder));
                    }
                }

                foreach (InventoryItemBase item in response.Items)
                {
                    items.Add(item);
                }
            }
            else
            {
                m_log.ErrorFormat("[INVENTORY CONNECTOR]: Did not get back an inventory containing a root folder for user {0}", userID);
            }

            callback(folders, items);

        }


    }
}
