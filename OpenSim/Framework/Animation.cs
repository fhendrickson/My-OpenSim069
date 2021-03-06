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
using OpenMetaverse.StructuredData;

namespace OpenSim.Framework
{
    [Serializable]
    public class Animation
    {
        private UUID animID;
        public UUID AnimID
        {
            get { return animID; }
            set { animID = value; }
        }

        private int sequenceNum;
        public int SequenceNum
        {
            get { return sequenceNum; }
            set { sequenceNum = value; }
        }

        private UUID objectID;
        public UUID ObjectID
        {
            get { return objectID; }
            set { objectID = value; }
        }

        public Animation()
        {
        }

        public Animation(UUID animID, int sequenceNum, UUID objectID)
        {
            this.animID = animID;
            this.sequenceNum = sequenceNum;
            this.objectID = objectID;
        }

        public Animation(OSDMap args)
        {
            UnpackUpdateMessage(args);
        }

        public OSDMap PackUpdateMessage()
        {
            OSDMap anim = new OSDMap();
            anim["animation"] = OSD.FromUUID(animID);
            anim["object_id"] = OSD.FromUUID(objectID);
            anim["seq_num"] = OSD.FromInteger(sequenceNum);
            return anim;
        }

        public void UnpackUpdateMessage(OSDMap args)
        {
            if (args["animation"] != null)
                animID = args["animation"].AsUUID();
            if (args["object_id"] != null)
                objectID = args["object_id"].AsUUID();
            if (args["seq_num"] != null)
                sequenceNum = args["seq_num"].AsInteger();
        }

    }
}
