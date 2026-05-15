using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class WhoisResponse
    {
        public string TargetAddress { get; set; } = string.Empty;
        public string? WhoisServer { get; set; }
        public string WhoisData { get; set; } = string.Empty;

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}
