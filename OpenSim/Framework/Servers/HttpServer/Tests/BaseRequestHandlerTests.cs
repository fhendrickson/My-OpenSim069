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
using System.Text;
using NUnit.Framework;
using OpenSim.Tests.Common.Setup;

namespace OpenSim.Framework.Servers.HttpServer.Tests
{
    [TestFixture]
    public class BaseRequestHandlerTests
    {
        private const string BASE_PATH = "/testpath";

        private class BaseRequestHandlerImpl : BaseRequestHandler
        {
            public BaseRequestHandlerImpl(string httpMethod, string path) : base(httpMethod, path)
            {
            }
        }

        [Test]
        public void TestConstructor()
        {
            new BaseRequestHandlerImpl(null, null);
        }

        [Test]
        public void TestGetParams()
        {
            BaseRequestHandlerImpl handler = new BaseRequestHandlerImpl(null, BASE_PATH);

            BaseRequestHandlerTestHelper.BaseTestGetParams(handler, BASE_PATH);
        }

        [Test]
        public void TestSplitParams()
        {
            BaseRequestHandlerImpl handler = new BaseRequestHandlerImpl(null, BASE_PATH);

            BaseRequestHandlerTestHelper.BaseTestSplitParams(handler, BASE_PATH);
        }
    }
}
