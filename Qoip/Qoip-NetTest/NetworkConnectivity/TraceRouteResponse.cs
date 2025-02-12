using Qoip.ZeroTrustNetwork.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class TraceRouteResponse
    {
        public string TargetAddress { get; set; }
        public int MaxHops { get; set; }
        public int Timeout { get; set; }
        public DetailLevel DetailLevel { get; set; }        
        public int TotalHops { get; set; }
        public int TotalResponseTime { get; set; }
        public List<TraceRouteResult> TraceResults { get; set; } = new List<TraceRouteResult>();

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }

    public class TraceRouteResult
    {
        public int Hop { get; set; }
        public string IpAddress { get; set; }
        public string Hostname { get; set; }
        public long RoundTripTime { get; set; }
        public string Status { get; set; }

    }
}
