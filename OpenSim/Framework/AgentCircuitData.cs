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
using System.Collections.Generic;
using OpenMetaverse;
using OpenMetaverse.StructuredData;

namespace OpenSim.Framework
{
    public class AgentCircuitData
    {
        public UUID AgentID;
        public AvatarAppearance Appearance;
        public UUID BaseFolder;
        public string CapsPath = String.Empty;
        public Dictionary<ulong, string> ChildrenCapSeeds;
        public bool child;
        public uint circuitcode;
        public string firstname;
        public UUID InventoryFolder;
        public string lastname;
        public UUID SecureSessionID;
        public UUID SessionID;
        public Vector3 startpos;

        public AgentCircuitData()
        {
        }

        public AgentCircuitData(sAgentCircuitData cAgent)
        {
            AgentID = new UUID(cAgent.AgentID);
            SessionID = new UUID(cAgent.SessionID);
            SecureSessionID = new UUID(cAgent.SecureSessionID);
            startpos = new Vector3(cAgent.startposx, cAgent.startposy, cAgent.startposz);
            firstname = cAgent.firstname;
            lastname = cAgent.lastname;
            circuitcode = cAgent.circuitcode;
            child = cAgent.child;
            InventoryFolder = new UUID(cAgent.InventoryFolder);
            BaseFolder = new UUID(cAgent.BaseFolder);
            CapsPath = cAgent.CapsPath;
            ChildrenCapSeeds = cAgent.ChildrenCapSeeds;
        }

        public OSDMap PackAgentCircuitData()
        {
            OSDMap args = new OSDMap();
            args["agent_id"] = OSD.FromUUID(AgentID);
            args["base_folder"] = OSD.FromUUID(BaseFolder);
            args["caps_path"] = OSD.FromString(CapsPath);

            OSDArray childrenSeeds = new OSDArray(ChildrenCapSeeds.Count);
            foreach (KeyValuePair<ulong, string> kvp in ChildrenCapSeeds)
            {
                OSDMap pair = new OSDMap();
                pair["handle"] = OSD.FromString(kvp.Key.ToString());
                pair["seed"] = OSD.FromString(kvp.Value);
                childrenSeeds.Add(pair);
            }
            if (ChildrenCapSeeds.Count > 0)
                args["children_seeds"] = childrenSeeds;

            args["child"] = OSD.FromBoolean(child);
            args["circuit_code"] = OSD.FromString(circuitcode.ToString());
            args["first_name"] = OSD.FromString(firstname);
            args["last_name"] = OSD.FromString(lastname);
            args["inventory_folder"] = OSD.FromUUID(InventoryFolder);
            args["secure_session_id"] = OSD.FromUUID(SecureSessionID);
            args["session_id"] = OSD.FromUUID(SessionID);
            args["start_pos"] = OSD.FromString(startpos.ToString());

            return args;
        }

        public void UnpackAgentCircuitData(OSDMap args)
        {
            if (args["agent_id"] != null)
                AgentID = args["agent_id"].AsUUID();
            if (args["base_folder"] != null)
                BaseFolder = args["base_folder"].AsUUID();
            if (args["caps_path"] != null)
                CapsPath = args["caps_path"].AsString();

            if ((args["children_seeds"] != null) && (args["children_seeds"].Type == OSDType.Array))
            {
                OSDArray childrenSeeds = (OSDArray)(args["children_seeds"]);
                ChildrenCapSeeds = new Dictionary<ulong, string>();
                foreach (OSD o in childrenSeeds)
                {
                    if (o.Type == OSDType.Map)
                    {
                        ulong handle = 0;
                        string seed = "";
                        OSDMap pair = (OSDMap)o;
                        if (pair["handle"] != null)
                            if (!UInt64.TryParse(pair["handle"].AsString(), out handle))
                                continue;
                        if (pair["seed"] != null)
                            seed = pair["seed"].AsString();
                        if (!ChildrenCapSeeds.ContainsKey(handle))
                            ChildrenCapSeeds.Add(handle, seed);
                    }
                }
            }

            if (args["child"] != null)
                child = args["child"].AsBoolean();
            if (args["circuit_code"] != null)
                UInt32.TryParse(args["circuit_code"].AsString(), out circuitcode);
            if (args["first_name"] != null)
                firstname = args["first_name"].AsString();
            if (args["last_name"] != null)
                lastname = args["last_name"].AsString();
            if (args["inventory_folder"] != null)
                InventoryFolder = args["inventory_folder"].AsUUID();
            if (args["secure_session_id"] != null)
                SecureSessionID = args["secure_session_id"].AsUUID();
            if (args["session_id"] != null)
                SessionID = args["session_id"].AsUUID();
            if (args["start_pos"] != null)
                Vector3.TryParse(args["start_pos"].AsString(), out startpos); 


        }
    }

    [Serializable]
    public class sAgentCircuitData
    {
        public Guid AgentID;
        public Guid BaseFolder;
        public string CapsPath = String.Empty;
        public Dictionary<ulong, string> ChildrenCapSeeds;
        public bool child;
        public uint circuitcode;
        public string firstname;
        public Guid InventoryFolder;
        public string lastname;
        public Guid SecureSessionID;
        public Guid SessionID;
        public float startposx;
        public float startposy;
        public float startposz;

        public sAgentCircuitData()
        {
        }

        public sAgentCircuitData(AgentCircuitData cAgent)
        {
            AgentID = cAgent.AgentID.Guid;
            SessionID = cAgent.SessionID.Guid;
            SecureSessionID = cAgent.SecureSessionID.Guid;
            startposx = cAgent.startpos.X;
            startposy = cAgent.startpos.Y;
            startposz = cAgent.startpos.Z;
            firstname = cAgent.firstname;
            lastname = cAgent.lastname;
            circuitcode = cAgent.circuitcode;
            child = cAgent.child;
            InventoryFolder = cAgent.InventoryFolder.Guid;
            BaseFolder = cAgent.BaseFolder.Guid;
            CapsPath = cAgent.CapsPath;
            ChildrenCapSeeds = cAgent.ChildrenCapSeeds;
        }
    }
}
