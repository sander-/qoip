using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class DnsSecAnalysisResponse
    {
        public string DomainName { get; set; } = string.Empty;
        public string DnsServer { get; set; } = string.Empty;
        public DetailLevel DetailLevel { get; set; }
        public bool HasDnsKey { get; set; }
        public bool HasRrSig { get; set; }
        public bool HasNsec { get; set; }
        public bool HasNsec3 { get; set; }
        public bool HasDs { get; set; }
        public List<string> DnsKeyRecords { get; set; } = new();
        public List<string> RrSigRecords { get; set; } = new();
        public List<string> NsecRecords { get; set; } = new();
        public List<string> Nsec3Records { get; set; } = new();
        public List<string> DsRecords { get; set; } = new();
        public List<string> Findings { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
