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
using OpenMetaverse;

namespace OpenSim.Framework
{
    /// <summary>
    /// Represents an item in a task inventory
    /// </summary>
    public class TaskInventoryItem : ICloneable
    {
        /// <summary>
        /// XXX This should really be factored out into some constants class.
        /// </summary>
        private const uint FULL_MASK_PERMISSIONS_GENERAL = 2147483647;

        /// <summary>
        /// Inventory types
        /// </summary>
        public static string[] InvTypes = new string[]
            {
                "texture",
                "sound",
                "calling_card",
                "landmark",
                String.Empty,
                String.Empty,
                "object",
                "notecard",
                String.Empty,
                String.Empty,
                "lsl_text",
                String.Empty,
                String.Empty,
                "bodypart",
                String.Empty,
                "snapshot",
                String.Empty,
                String.Empty,
                "wearable",
                "animation",
                "gesture"
            };

        /// <summary>
        /// Asset types
        /// </summary>
        public static string[] Types = new string[]
            {
                "texture",
                "sound",
                "callcard",
                "landmark",
                "clothing", // Deprecated
                "clothing",
                "object",
                "notecard",
                "category",
                "root",
                "lsltext",
                "lslbyte",
                "txtr_tga",
                "bodypart",
                "trash",
                "snapshot",
                "lstndfnd",
                "snd_wav",
                "img_tga",
                "jpeg",
                "animatn",
                "gesture"
            };

        private UUID _assetID = UUID.Zero;

        private uint _baseMask = FULL_MASK_PERMISSIONS_GENERAL;
        private uint _creationDate = 0;
        private UUID _creatorID = UUID.Zero;
        private string _description = String.Empty;
        private uint _everyoneMask = FULL_MASK_PERMISSIONS_GENERAL;
        private uint _flags = 0;
        private UUID _groupID = UUID.Zero;
        private uint _groupMask = FULL_MASK_PERMISSIONS_GENERAL;

        private int _invType = 0;
        private UUID _itemID = UUID.Zero;
        private UUID _lastOwnerID = UUID.Zero;
        private string _name = String.Empty;
        private uint _nextOwnerMask = FULL_MASK_PERMISSIONS_GENERAL;
        private UUID _ownerID = UUID.Zero;
        private uint _ownerMask = FULL_MASK_PERMISSIONS_GENERAL;
        private UUID _parentID = UUID.Zero; //parent folder id
        private UUID _parentPartID = UUID.Zero; // SceneObjectPart this is inside
        private UUID _permsGranter;
        private int _permsMask;
        private int _type = 0;
        private UUID _oldID;

        public UUID AssetID {
            get {
                return _assetID;
            }
            set {
                _assetID = value;
            }
        }

        public uint BasePermissions {
            get {
                return _baseMask;
            }
            set {
                _baseMask = value;
            }
        }

        public uint CreationDate {
            get {
                return _creationDate;
            }
            set {
                _creationDate = value;
            }
        }

        public UUID CreatorID {
            get {
                return _creatorID;
            }
            set {
                _creatorID = value;
            }
        }

        public string Description {
            get {
                return _description;
            }
            set {
                _description = value;
            }
        }

        public uint EveryonePermissions {
            get {
                return _everyoneMask;
            }
            set {
                _everyoneMask = value;
            }
        }

        public uint Flags {
            get {
                return _flags;
            }
            set {
                _flags = value;
            }
        }

        public UUID GroupID {
            get {
                return _groupID;
            }
            set {
                _groupID = value;
            }
        }

        public uint GroupPermissions {
            get {
                return _groupMask;
            }
            set {
                _groupMask = value;
            }
        }

        public int InvType {
            get {
                return _invType;
            }
            set {
                _invType = value;
            }
        }

        public UUID ItemID {
            get {
                return _itemID;
            }
            set {
                _itemID = value;
            }
        }

        public UUID OldItemID {
            get {
                return _oldID;
            }
            set {
                _oldID = value;
            }
        }

        public UUID LastOwnerID {
            get {
                return _lastOwnerID;
            }
            set {
                _lastOwnerID = value;
            }
        }

        public string Name {
            get {
                return _name;
            }
            set {
                _name = value;
            }
        }

        public uint NextPermissions {
            get {
                return _nextOwnerMask;
            }
            set {
                _nextOwnerMask = value;
            }
        }

        public UUID OwnerID {
            get {
                return _ownerID;
            }
            set {
                _ownerID = value;
            }
        }

        public uint CurrentPermissions {
            get {
                return _ownerMask;
            }
            set {
                _ownerMask = value;
            }
        }

        public UUID ParentID {
            get {
                return _parentID;
            }
            set {
                _parentID = value;
            }
        }

        public UUID ParentPartID {
            get {
                return _parentPartID;
            }
            set {
                _parentPartID = value;
            }
        }

        public UUID PermsGranter {
            get {
                return _permsGranter;
            }
            set {
                _permsGranter = value;
            }
        }

        public int PermsMask {
            get {
                return _permsMask;
            }
            set {
                _permsMask = value;
            }
        }

        public int Type {
            get {
                return _type;
            }
            set {
                _type = value;
            }
        }

        // See ICloneable

        #region ICloneable Members

        public Object Clone()
        {
            return MemberwiseClone();
        }

        #endregion

        /// <summary>
        /// Reset the UUIDs for this item.
        /// </summary>
        /// <param name="partID">The new part ID to which this item belongs</param>
        public void ResetIDs(UUID partID)
        {
            _oldID  = _itemID;
            _itemID = UUID.Random();
            _parentPartID = partID;
            _parentID = partID;
        }

        public TaskInventoryItem()
        {
            _creationDate = (uint)(DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalSeconds;
        }
    }
}
