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
using System.Text;

using OpenMetaverse;
using OpenMetaverse.StructuredData;


namespace OpenSim.Framework.Statistics
{
    /// <summary>
    /// Statistics which all collectors are interested in reporting
    /// </summary>
    public class BaseStatsCollector : IStatsCollector
    {
        public virtual string Report()
        {
            StringBuilder sb = new StringBuilder(Environment.NewLine);
            sb.Append("MEMORY STATISTICS");
            sb.Append(Environment.NewLine);
            sb.Append(
                string.Format(
                    "Allocated to OpenSim : {0} MB" + Environment.NewLine,
                    Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0)));

            return sb.ToString();
        }
        
        public virtual string XReport(string uptime, string version)
        {
            return (string) Math.Round(GC.GetTotalMemory(false) / 1024.0 / 1024.0).ToString() ;
        }
    }
}
