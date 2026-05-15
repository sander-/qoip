using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class HttpProbeResponse
    {
        public string Url { get; set; } = string.Empty;
        public string FinalUrl { get; set; } = string.Empty;
        public int StatusCode { get; set; }
        public string ReasonPhrase { get; set; } = string.Empty;
        public bool IsSuccessStatusCode { get; set; }
        public bool IsHttps { get; set; }
        public string Server { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DetailLevel DetailLevel { get; set; }
        public long TotalResponseTime { get; set; }
        public List<string> RedirectChain { get; set; } = new();
        public Dictionary<string, string> Headers { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
