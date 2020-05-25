// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;

namespace WinDynamicDesktop
{
    class ProxyWrapper
    {
        private static IWebProxy webProxy;

        private static IWebProxy GetProxy()
        {
            // Ensure TLS 1.2 is enabled
            if (!ServicePointManager.SecurityProtocol.HasFlag(SecurityProtocolType.Tls12))
            {
                ServicePointManager.SecurityProtocol |= SecurityProtocolType.Tls12;
            }
            
            if (webProxy == null)
            {
                // Uses HttpEnvironmentProxy class to mimic .NET Core behavior
                HttpEnvironmentProxy.TryCreate(out webProxy);
            }

            return webProxy;
        }

        public static void ApplyProxyToClient(RestSharp.RestClient client)
        {
            IWebProxy proxy = GetProxy();
            
            if (proxy != null)
            {
                client.Proxy = proxy;
            }
        }

        public static void ApplyProxyToClient(WebClient client)
        {
            IWebProxy proxy = GetProxy();

            if (proxy != null)
            {
                client.Proxy = proxy;
            }
        }
    }
}
