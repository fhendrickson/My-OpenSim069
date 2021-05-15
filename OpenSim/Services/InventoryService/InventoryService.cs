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

using System.Collections.Generic;
using System.Reflection;
using log4net;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Data;
using OpenSim.Framework;
using OpenSim.Services.Interfaces;

namespace OpenSim.Services.InventoryService
{
    /// <summary>
    /// The Inventory service reference implementation
    /// </summary>
    public class InventoryService : InventoryServiceBase, IInventoryService
    {
        private static readonly ILog m_log
            = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public InventoryService(IConfigSource config) : base(config)
        {
            m_log.Debug("[INVENTORY SERVICE]: Initialized.");
        }

        #region IInventoryServices methods

        public string Host
        {
            get { return "default"; }
        }

        public List<InventoryFolderBase> GetInventorySkeleton(UUID userId)
        {
            m_log.DebugFormat("[INVENTORY SERVICE]: Getting inventory skeleton for {0}", userId);

            InventoryFolderBase rootFolder = RequestRootFolder(userId);

            // Agent has no inventory structure yet.
            if (null == rootFolder)
            {
                return null;
            }

            List<InventoryFolderBase> userFolders = new List<InventoryFolderBase>();

            userFolders.Add(rootFolder);

            IList<InventoryFolderBase> folders = m_Database.getFolderHierarchy(rootFolder.ID);
            userFolders.AddRange(folders);

//            m_log.DebugFormat("[INVENTORY SERVICE]: Got folder {0} {1}", folder.name, folder.folderID);

            return userFolders;
        }

        public virtual bool HasInventoryForUser(UUID userID)
        {
            return false;
        }

        // See IInventoryServices
        public virtual InventoryFolderBase RequestRootFolder(UUID userID)
        {
            // Retrieve the first root folder we get from the DB.
            InventoryFolderBase rootFolder = m_Database.getUserRootFolder(userID);
            if (rootFolder != null)
                return rootFolder;

            // Return nothing if the plugin was unable to supply a root folder
            return null;
        }

        // See IInventoryServices
        public bool CreateUserInventory(UUID user)
        {
            InventoryFolderBase existingRootFolder = RequestRootFolder(user);

            if (null != existingRootFolder)
            {
                m_log.WarnFormat(
                    "[INVENTORY SERVICE]: Did not create a new inventory for user {0} since they already have "
                    + "a root inventory folder with id {1}",
                    user, existingRootFolder.ID);
            }
            else
            {
                UsersInventory inven = new UsersInventory();
                inven.CreateNewInventorySet(user);
                AddNewInventorySet(inven);

                return true;
            }

            return false;
        }

        // See IInventoryServices

        /// <summary>
        /// Return a user's entire inventory synchronously
        /// </summary>
        /// <param name="rawUserID"></param>
        /// <returns>The user's inventory.  If an inventory cannot be found then an empty collection is returned.</returns>
        public InventoryCollection GetUserInventory(UUID userID)
        {
            m_log.InfoFormat("[INVENTORY SERVICE]: Processing request for inventory of {0}", userID);

            // Uncomment me to simulate a slow responding inventory server
            //Thread.Sleep(16000);

            InventoryCollection invCollection = new InventoryCollection();

            List<InventoryFolderBase> allFolders = GetInventorySkeleton(userID);

            if (null == allFolders)
            {
                m_log.WarnFormat("[INVENTORY SERVICE]: No inventory found for user {0}", userID);

                return invCollection;
            }

            List<InventoryItemBase> allItems = new List<InventoryItemBase>();

            foreach (InventoryFolderBase folder in allFolders)
            {
                List<InventoryItemBase> items = GetFolderItems(userID, folder.ID);

                if (items != null)
                {
                    allItems.InsertRange(0, items);
                }
            }

            invCollection.UserID = userID;
            invCollection.Folders = allFolders;
            invCollection.Items = allItems;

            //            foreach (InventoryFolderBase folder in invCollection.Folders)
            //            {
            //                m_log.DebugFormat("[GRID INVENTORY SERVICE]: Sending back folder {0} {1}", folder.Name, folder.ID);
            //            }
            //
            //            foreach (InventoryItemBase item in invCollection.Items)
            //            {
            //                m_log.DebugFormat("[GRID INVENTORY SERVICE]: Sending back item {0} {1}, folder {2}", item.Name, item.ID, item.Folder);
            //            }

            m_log.InfoFormat(
                "[INVENTORY SERVICE]: Sending back inventory response to user {0} containing {1} folders and {2} items",
                invCollection.UserID, invCollection.Folders.Count, invCollection.Items.Count);

            return invCollection;
        }

        /// <summary>
        /// Asynchronous inventory fetch.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="callback"></param>
        public void GetUserInventory(UUID userID, InventoryReceiptCallback callback)
        {
            m_log.InfoFormat("[INVENTORY SERVICE]: Requesting inventory for user {0}", userID);

            List<InventoryFolderImpl> folders = new List<InventoryFolderImpl>();
            List<InventoryItemBase> items = new List<InventoryItemBase>();

            List<InventoryFolderBase> skeletonFolders = GetInventorySkeleton(userID);

            if (skeletonFolders != null)
            {

                InventoryFolderImpl rootFolder = null;

                // Need to retrieve the root folder on the first pass
                foreach (InventoryFolderBase folder in skeletonFolders)
                {
                    if (folder.ParentID == UUID.Zero)
                    {
                        rootFolder = new InventoryFolderImpl(folder);
                        folders.Add(rootFolder);
                        items.AddRange(GetFolderItems(userID, rootFolder.ID));
                        break; // Only 1 root folder per user
                    }
                }

                if (rootFolder != null)
                {
                    foreach (InventoryFolderBase folder in skeletonFolders)
                    {
                        if (folder.ID != rootFolder.ID)
                        {
                            folders.Add(new InventoryFolderImpl(folder));
                            items.AddRange(GetFolderItems(userID, folder.ID));
                        }
                    }
                }

                m_log.InfoFormat(
                    "[INVENTORY SERVICE]: Received inventory response for user {0} containing {1} folders and {2} items",
                    userID, folders.Count, items.Count);
            }
            else
            {
                m_log.WarnFormat("[INVENTORY SERVICE]: User {0} inventory not available", userID);
            }

            callback.BeginInvoke(folders, items, null, null);
        }

        public List<InventoryItemBase> GetActiveGestures(UUID userId)
        {
            List<InventoryItemBase> activeGestures = new List<InventoryItemBase>();
            activeGestures.AddRange(m_Database.fetchActiveGestures(userId));
            
            return activeGestures;
        }

        #endregion

        #region Methods used by GridInventoryService

        public List<InventoryFolderBase> RequestSubFolders(UUID parentFolderID)
        {
            List<InventoryFolderBase> inventoryList = new List<InventoryFolderBase>();
            
            inventoryList.AddRange(m_Database.getInventoryFolders(parentFolderID));
            
            return inventoryList;
        }

        public List<InventoryItemBase> GetFolderItems(UUID userID, UUID folderID)
        {
            List<InventoryItemBase> itemsList = new List<InventoryItemBase>();
            
            itemsList.AddRange(m_Database.getInventoryInFolder(folderID));
            
            return itemsList;
        }

        #endregion

        // See IInventoryServices
        public virtual bool AddFolder(InventoryFolderBase folder)
        {
            m_log.DebugFormat(
                "[INVENTORY SERVICE]: Adding folder {0} {1} to folder {2}", folder.Name, folder.ID, folder.ParentID);

            m_Database.addInventoryFolder(folder);

            // FIXME: Should return false on failure
            return true;
        }

        // See IInventoryServices
        public virtual bool UpdateFolder(InventoryFolderBase folder)
        {
            m_log.DebugFormat(
                "[INVENTORY SERVICE]: Updating folder {0} {1} to folder {2}", folder.Name, folder.ID, folder.ParentID);

            m_Database.updateInventoryFolder(folder);

            // FIXME: Should return false on failure
            return true;
        }

        // See IInventoryServices
        public virtual bool MoveFolder(InventoryFolderBase folder)
        {
            m_log.DebugFormat(
                "[INVENTORY SERVICE]: Moving folder {0} {1} to folder {2}", folder.Name, folder.ID, folder.ParentID);

            m_Database.moveInventoryFolder(folder);

            // FIXME: Should return false on failure
            return true;
        }

        // See IInventoryServices
        public virtual bool AddItem(InventoryItemBase item)
        {
            m_log.DebugFormat(
                "[INVENTORY SERVICE]: Adding item {0} {1} to folder {2}", item.Name, item.ID, item.Folder);

            m_Database.addInventoryItem(item);

            // FIXME: Should return false on failure
            return true;
        }

        // See IInventoryServices
        public virtual bool UpdateItem(InventoryItemBase item)
        {
            m_log.InfoFormat(
                "[INVENTORY SERVICE]: Updating item {0} {1} in folder {2}", item.Name, item.ID, item.Folder);

            m_Database.updateInventoryItem(item);

            // FIXME: Should return false on failure
            return true;
        }

        // See IInventoryServices
        public virtual bool DeleteItem(InventoryItemBase item)
        {
            m_log.InfoFormat(
                "[INVENTORY SERVICE]: Deleting item {0} {1} from folder {2}", item.Name, item.ID, item.Folder);

            m_Database.deleteInventoryItem(item.ID);

            // FIXME: Should return false on failure
            return true;
        }

        public virtual InventoryItemBase QueryItem(InventoryItemBase item)
        {
            InventoryItemBase result = m_Database.queryInventoryItem(item.ID);
            if (result != null)
                return result;

            return null;
        }

        public virtual InventoryFolderBase QueryFolder(InventoryFolderBase item)
        {
            InventoryFolderBase result = m_Database.queryInventoryFolder(item.ID);
            if (result != null)
                return result;

            return null;
        }

        /// <summary>
        /// Purge a folder of all items items and subfolders.
        ///
        /// FIXME: Really nasty in a sense, because we have to query the database to get information we may
        /// already know...  Needs heavy refactoring.
        /// </summary>
        /// <param name="folder"></param>
        public virtual bool PurgeFolder(InventoryFolderBase folder)
        {
            m_log.DebugFormat(
                "[INVENTORY SERVICE]: Purging folder {0} {1} of its contents", folder.Name, folder.ID);

            List<InventoryFolderBase> subFolders = RequestSubFolders(folder.ID);

            foreach (InventoryFolderBase subFolder in subFolders)
            {
//                m_log.DebugFormat("[INVENTORY SERVICE]: Deleting folder {0} {1}", subFolder.Name, subFolder.ID);

                m_Database.deleteInventoryFolder(subFolder.ID);
            }

            List<InventoryItemBase> items = GetFolderItems(folder.Owner, folder.ID);

            foreach (InventoryItemBase item in items)
            {
                DeleteItem(item);
            }

            // FIXME: Should return false on failure
            return true;
        }

        private void AddNewInventorySet(UsersInventory inventory)
        {
            foreach (InventoryFolderBase folder in inventory.Folders.Values)
            {
                AddFolder(folder);
            }
        }

        public InventoryItemBase GetInventoryItem(UUID itemID)
        {
            InventoryItemBase item = m_Database.getInventoryItem(itemID);
            if (item != null)
                return item;

            return null;
        }

        /// <summary>
        /// Used to create a new user inventory.
        /// </summary>
        private class UsersInventory
        {
            public Dictionary<UUID, InventoryFolderBase> Folders = new Dictionary<UUID, InventoryFolderBase>();
            public Dictionary<UUID, InventoryItemBase> Items = new Dictionary<UUID, InventoryItemBase>();

            public virtual void CreateNewInventorySet(UUID user)
            {
                InventoryFolderBase folder = new InventoryFolderBase();

                folder.ParentID = UUID.Zero;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "My Inventory";
                folder.Type = (short)AssetType.Folder;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                UUID rootFolder = folder.ID;

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Animations";
                folder.Type = (short)AssetType.Animation;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Body Parts";
                folder.Type = (short)AssetType.Bodypart;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Calling Cards";
                folder.Type = (short)AssetType.CallingCard;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Clothing";
                folder.Type = (short)AssetType.Clothing;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Gestures";
                folder.Type = (short)AssetType.Gesture;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Landmarks";
                folder.Type = (short)AssetType.Landmark;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Lost And Found";
                folder.Type = (short)AssetType.LostAndFoundFolder;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Notecards";
                folder.Type = (short)AssetType.Notecard;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Objects";
                folder.Type = (short)AssetType.Object;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Photo Album";
                folder.Type = (short)AssetType.SnapshotFolder;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Scripts";
                folder.Type = (short)AssetType.LSLText;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Sounds";
                folder.Type = (short)AssetType.Sound;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Textures";
                folder.Type = (short)AssetType.Texture;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);

                folder = new InventoryFolderBase();
                folder.ParentID = rootFolder;
                folder.Owner = user;
                folder.ID = UUID.Random();
                folder.Name = "Trash";
                folder.Type = (short)AssetType.TrashFolder;
                folder.Version = 1;
                Folders.Add(folder.ID, folder);
            }
        }
    }
}
