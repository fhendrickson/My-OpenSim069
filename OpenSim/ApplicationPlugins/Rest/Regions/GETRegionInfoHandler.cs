/// <license>
/// Copyright (c) Contributors, https://hyperionvirtual.com/
/// See CONTRIBUTORS.TXT for a full list of copyright holders.
/// 
/// Redistribution and use in source and binary forms, with or without
/// modification, are permitted provided that the following conditions are met:
///     * Redistributions of source code must retain the above copyright
///     notice, this list of conditions and the following disclaimer.
///     * Redistributions in binary form must reproduce the above copyright
///     notice, this list of conditions and the following disclaimer in the
///     documentation and/or other materials provided with the distribution.
///     * Neither the name of the Hyperion Virtual Worlds Project nor the
///     names of its contributors may be used to endorse or promote products
///     derived from this software without specific prior written permission.
///     
/// THIS SOFTWARE IS PROVIDED BY THE DEVELOPERS ``AS IS'' AND ANY
/// EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
/// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
/// DISCLAIMED. IN NO EVENT SHALL THE CONTRIBUTORS BE LIABLE FOR ANY
/// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
/// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
/// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
/// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
/// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
/// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
/// </license>

using System;
using System.IO;
using System.Xml.Serialization;
using OpenMetaverse;
using OpenSim.Framework.Servers;
using OpenSim.Framework.Servers.HttpServer;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.Framework.Scenes;

namespace OpenSim.ApplicationPlugins.Rest.Regions
{
    public partial class RestRegionPlugin : RestPlugin
    {
        #region GET methods
        
        public string GetRegionInfoHandler(string request, string path, string param, OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            MsgID = RequestID;
            m_log.DebugFormat("{0} GET path {1} param {2}", MsgID, path, param);

            try
            {
                // param empty: regions list 
                return GetRegionInfoHandlerRegions(httpResponse);
            }
            catch (Exception e)
            {
                return Failure(httpResponse, OSHttpStatusCode.ServerErrorInternalError, "GET", e);
            }
        }

        public string GetRegionInfoHandlerRegions(OSHttpResponse httpResponse)
        {
            RestXmlWriter rxw = new RestXmlWriter(new StringWriter());

            // regions info
            rxw.WriteStartElement(String.Empty, "regions", String.Empty);
            {
                // regions info: number of regions
                rxw.WriteStartAttribute(String.Empty, "number", String.Empty);
                rxw.WriteValue(App.SceneManager.Scenes.Count);
                rxw.WriteEndAttribute();

                // regions info: max number of regions
                rxw.WriteStartAttribute(String.Empty, "max", String.Empty);
        
                if (App.ConfigSource.Source.Configs["RemoteAdmin"] != null)
                {
                    rxw.WriteValue(App.ConfigSource.Source.Configs["RemoteAdmin"].GetInt("region_limit", -1));
                }
                else
                {
                    rxw.WriteValue(-1);
                }
                
                rxw.WriteEndAttribute();
                
                // regions info: region
                foreach (Scene s in App.SceneManager.Scenes)
                {
                    rxw.WriteStartElement(String.Empty, "region", String.Empty);
                    
                    rxw.WriteStartAttribute(String.Empty, "uuid", String.Empty);
                    rxw.WriteString(s.RegionInfo.RegionID.ToString());
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "name", String.Empty);
                    rxw.WriteString(s.RegionInfo.RegionName);
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "x", String.Empty);
                    rxw.WriteValue(s.RegionInfo.RegionLocX);
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "y", String.Empty);
                    rxw.WriteValue(s.RegionInfo.RegionLocY);
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "external_hostname", String.Empty);
                    rxw.WriteString(s.RegionInfo.ExternalHostName);
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "master_name", String.Empty);
                    rxw.WriteString(String.Format("{0} {1}", s.RegionInfo.MasterAvatarFirstName, s.RegionInfo.MasterAvatarLastName));
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "master_uuid", String.Empty);
                    rxw.WriteString(s.RegionInfo.MasterAvatarAssignedUUID.ToString());
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "ip", String.Empty);
                    rxw.WriteString(s.RegionInfo.InternalEndPoint.ToString());
                    rxw.WriteEndAttribute();
                    
                    int users = s.GetAvatars().Count;
                    rxw.WriteStartAttribute(String.Empty, "avatars", String.Empty);
                    rxw.WriteValue(users);
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteStartAttribute(String.Empty, "objects", String.Empty);
                    rxw.WriteValue(s.Entities.Count - users);
                    rxw.WriteEndAttribute();
                    
                    rxw.WriteEndElement();
                }
            }

            return rxw.ToString();
        }

        #endregion GET methods
    }
}
