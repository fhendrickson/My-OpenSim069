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

using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace OpenSim.Framework.Servers.HttpServer
{
    public delegate TResponse RestDeserialiseMethod<TRequest, TResponse>(TRequest request);

    public class RestDeserialiseHandler<TRequest, TResponse> : BaseRequestHandler, IStreamHandler
        where TRequest : new()
    {
        private RestDeserialiseMethod<TRequest, TResponse> m_method;

        public RestDeserialiseHandler(string httpMethod, string path, RestDeserialiseMethod<TRequest, TResponse> method)
            : base(httpMethod, path)
        {
            m_method = method;
        }

        public void Handle(string path, Stream request, Stream responseStream,
                           OSHttpRequest httpRequest, OSHttpResponse httpResponse)
        {
            TRequest deserial;
            using (XmlTextReader xmlReader = new XmlTextReader(request))
            {
                XmlSerializer deserializer = new XmlSerializer(typeof (TRequest));
                deserial = (TRequest) deserializer.Deserialize(xmlReader);
            }

            TResponse response = m_method(deserial);

            using (XmlWriter xmlWriter = XmlTextWriter.Create(responseStream))
            {
                XmlSerializer serializer = new XmlSerializer(typeof (TResponse));
                serializer.Serialize(xmlWriter, response);
            }
        }
    }
}
