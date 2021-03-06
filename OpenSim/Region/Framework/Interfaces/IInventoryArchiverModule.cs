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
using System.IO;
using OpenSim.Framework.Communications.Cache;

namespace OpenSim.Region.Framework.Interfaces
{    
    /// <summary>
    /// Used for the OnInventoryArchiveSaved event.
    /// </summary>
    /// <param name="succeeded">true if the save succeeded, false otherwise</param>
    /// <param name="userInfo">The user for whom the save was conducted</param>
    /// <param name="invPath">The inventory path saved</param>
    /// <param name="savePath">The stream to which the archive was saved</param>
    /// <param name="reportedException">Contains the exception generated if the save did not succeed</param>
    public delegate void InventoryArchiveSaved(
        bool succeeded, CachedUserInfo userInfo, string invPath, Stream saveStream, Exception reportedException);
    
    public interface IInventoryArchiverModule       
    {
        /// <summary>
        /// Fired when an archive inventory save has been completed.
        /// </summary>        
        event InventoryArchiveSaved OnInventoryArchiveSaved;
        
        /// <summary>
        /// Dearchive a user's inventory folder from the given stream
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="invPath">The inventory path in which to place the loaded folders and items</param>
        /// <param name="loadStream">The stream from which the inventory archive will be loaded</param>        
        void DearchiveInventory(string firstName, string lastName, string invPath, Stream loadStream);

        /// <summary>
        /// Archive a user's inventory folder to the given stream
        /// </summary>
        /// <param name="firstName"></param>
        /// <param name="lastName"></param>
        /// <param name="invPath">The inventory path from which the inventory should be saved.</param>
        /// <param name="saveStream">The stream to which the inventory archive will be saved</param>        
        void ArchiveInventory(string firstName, string lastName, string invPath, Stream saveStream);     
    }
}
