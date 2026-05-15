using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class RangeScanResponse
    {
        public string InputRange { get; set; } = string.Empty;
        public string PortSet { get; set; } = string.Empty;
        public DetailLevel DetailLevel { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public Dictionary<string, List<int>> HostsWithOpenPorts { get; set; } = new();
        public List<string> ScannedHosts { get; set; } = new();
        public double ExecutionTimeMilliseconds => Math.Round((ScanEndTime - ScanStartTime).TotalMilliseconds);

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
