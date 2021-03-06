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

using System.Reflection;
using log4net;
using Nini.Config;
using OpenMetaverse;
using OpenSim.Framework;
using OpenSim.Framework.Communications.Cache;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.Region.CoreModules.Avatar.Gestures
{
    public class GesturesModule : IRegionModule
    { 
        private static readonly ILog m_log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        
        protected Scene m_scene;
        
        public void Initialise(Scene scene, IConfigSource source)
        {
            m_scene = scene;
            
            m_scene.EventManager.OnNewClient += OnNewClient;
        }
        
        public void PostInitialise() {}
        public void Close() {}
        public string Name { get { return "Gestures Module"; } }
        public bool IsSharedModule { get { return false; } }
        
        private void OnNewClient(IClientAPI client)
        {
            client.OnActivateGesture += ActivateGesture;
            client.OnDeactivateGesture += DeactivateGesture;
        }
        
        public virtual void ActivateGesture(IClientAPI client, UUID assetId, UUID gestureId)
        {
            CachedUserInfo userInfo = m_scene.CommsManager.UserProfileCacheService.GetUserDetails(client.AgentId);

            if (userInfo != null)
            {
                InventoryItemBase item = userInfo.RootFolder.FindItem(gestureId);
                if (item != null)
                {
                    item.Flags = 1;
                    userInfo.UpdateItem(item);
                }
                else 
                    m_log.ErrorFormat(
                        "[GESTURES]: Unable to find gesture to activate {0} for {1}", gestureId, client.Name);
            }
            else 
                m_log.ErrorFormat("[GESTURES]: Unable to find user {0}", client.Name);
        }

        public virtual void DeactivateGesture(IClientAPI client, UUID gestureId)
        {
            CachedUserInfo userInfo = m_scene.CommsManager.UserProfileCacheService.GetUserDetails(client.AgentId);

            if (userInfo != null)
            {
                InventoryItemBase item = userInfo.RootFolder.FindItem(gestureId);
                if (item != null)
                {
                    item.Flags = 0;
                    userInfo.UpdateItem(item);
                }
                else 
                    m_log.ErrorFormat(
                        "[GESTURES]: Unable to find gesture to deactivate {0} for {1}", gestureId, client.Name);
            }
            else 
                m_log.ErrorFormat("[GESTURES]: Unable to find user {0}", client.Name);
        }        
    }
}