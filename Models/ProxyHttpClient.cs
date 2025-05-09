using System.Net.Http;
using System.Net;

namespace MapFusion.Models
{
    public class ProxyHttpClient
    {
        public static HttpClient CreateClient(string proxyUrl)
        {
            var httpClientHandler = new HttpClientHandler()
            {
                Proxy = new WebProxy(proxyUrl),
                UseProxy = true
            };
            return new HttpClient(httpClientHandler);
        }
    }
}
