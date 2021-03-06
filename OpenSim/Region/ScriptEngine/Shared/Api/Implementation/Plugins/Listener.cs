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
using log4net;
using OpenMetaverse;
using OpenSim.Region.Framework.Interfaces;
using OpenSim.Region.CoreModules.Scripting.WorldComm;
using OpenSim.Region.ScriptEngine.Interfaces;
using OpenSim.Region.ScriptEngine.Shared;
using OpenSim.Region.ScriptEngine.Shared.Api;

namespace OpenSim.Region.ScriptEngine.Shared.Api.Plugins
{
    public class Listener
    {
        // private static readonly ILog m_log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public AsyncCommandManager m_CmdManager;

        public Listener(AsyncCommandManager CmdManager)
        {
            m_CmdManager = CmdManager;
        }

        public void CheckListeners()
        {
            if (m_CmdManager.m_ScriptEngine.World == null)
                return;
            IWorldComm comms = m_CmdManager.m_ScriptEngine.World.RequestModuleInterface<IWorldComm>();

            if (comms != null)
            {
                while (comms.HasMessages())
                {
                    ListenerInfo lInfo = (ListenerInfo)comms.GetNextMessage();

                    //Deliver data to prim's listen handler
                    object[] resobj = new object[]
                    {
                        new LSL_Types.LSLInteger(lInfo.GetChannel()),
                        new LSL_Types.LSLString(lInfo.GetName()),
                        new LSL_Types.LSLString(lInfo.GetID().ToString()),
                        new LSL_Types.LSLString(lInfo.GetMessage())
                    };

                    foreach (IScriptEngine e in m_CmdManager.ScriptEngines)
                    {
                        e.PostScriptEvent(
                                lInfo.GetItemID(), new EventParams(
                                "listen", resobj,
                                new DetectParams[0]));
                    }
                }
            }
        }

        public Object[] GetSerializationData(UUID itemID)
        {
            IWorldComm comms = m_CmdManager.m_ScriptEngine.World.RequestModuleInterface<IWorldComm>();

            return comms.GetSerializationData(itemID);
        }

        public void CreateFromData(uint localID, UUID itemID, UUID hostID,
                Object[] data)
        {
            IWorldComm comms = m_CmdManager.m_ScriptEngine.World.RequestModuleInterface<IWorldComm>();

            comms.CreateFromData(localID, itemID, hostID, data);
        }
    }
}
