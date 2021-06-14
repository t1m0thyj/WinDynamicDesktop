// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System.Net;
using System.Net.Http;

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
                webProxy = HttpClient.DefaultProxy;
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
