using Qoip.ZeroTrustNetwork.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class PortScanResponse
    {
        public string IpAddress { get; set; }
        public DateTime ScanStartTime { get; set; }
        public DateTime ScanEndTime { get; set; }
        public DetailLevel DetailLevel { get; set; }

        [JsonIgnore]
        public Dictionary<int, bool> UnorderedPortResults { get; set; }
        public List<int> OpenPorts => UnorderedPortResults != null ? new List<int>(UnorderedPortResults.Where(kv => kv.Value).Select(kv => kv.Key)) : new List<int>();
        public double ExecutionTimeMilliseconds => Math.Round((ScanEndTime - ScanStartTime).TotalMilliseconds);

        [JsonIgnore]
        public Dictionary<int, bool> PortResults => UnorderedPortResults != null ? UnorderedPortResults.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value) : new Dictionary<int, bool>();

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}



