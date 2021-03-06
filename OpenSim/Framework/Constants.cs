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

namespace OpenSim.Framework
{
    public class Constants
    {
        public const uint RegionSize = 256;
        public const byte TerrainPatchSize = 16;
        public const string DefaultTexture = "89556747-24cb-43ed-920b-47caed15465f";

        public enum EstateAccessCodex : uint
        {
            AccessOptions = 17,
            AllowedGroups = 18,
            EstateBans = 20,
            EstateManagers = 24
        }

        [Flags]public enum TeleportFlags : uint
        {
            /// <summary>No flags set, or teleport failed</summary>
            Default = 0,
            /// <summary>Set when newbie leaves help island for first time</summary>
            SetHomeToTarget = 1 << 0,
            /// <summary></summary>
            SetLastToTarget = 1 << 1,
            /// <summary>Via Lure</summary>
            ViaLure = 1 << 2,
            /// <summary>Via Landmark</summary>
            ViaLandmark = 1 << 3,
            /// <summary>Via Location</summary>
            ViaLocation = 1 << 4,
            /// <summary>Via Home</summary>
            ViaHome = 1 << 5,
            /// <summary>Via Telehub</summary>
            ViaTelehub = 1 << 6,
            /// <summary>Via Login</summary>
            ViaLogin = 1 << 7,
            /// <summary>Linden Summoned</summary>
            ViaGodlikeLure = 1 << 8,
            /// <summary>Linden Forced me</summary>
            Godlike = 1 << 9,
            /// <summary></summary>
            NineOneOne = 1 << 10,
            /// <summary>Agent Teleported Home via Script</summary>
            DisableCancel = 1 << 11,
            /// <summary></summary>
            ViaRegionID = 1 << 12,
            /// <summary></summary>
            IsFlying = 1 << 13,
            /// <summary></summary>
            ResetHome = 1 << 14,
            /// <summary>forced to new location for example when avatar is banned or ejected</summary>
            ForceRedirect = 1 << 15,
            /// <summary>Teleport Finished via a Lure</summary>
            FinishedViaLure = 1 << 26,
            /// <summary>Finished, Sim Changed</summary>
            FinishedViaNewSim = 1 << 28,
            /// <summary>Finished, Same Sim</summary>
            FinishedViaSameSim = 1 << 29
        }

    }
}
