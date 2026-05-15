using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkSecurity
{
    public class EmailSecurityAnalysisResponse
    {
        public string DomainName { get; set; } = string.Empty;
        public DetailLevel DetailLevel { get; set; }
        public List<string> MxRecords { get; set; } = new();
        public List<string> SpfRecords { get; set; } = new();
        public string DmarcRecord { get; set; } = string.Empty;
        public string DkimRecord { get; set; } = string.Empty;
        public bool HasMxRecords { get; set; }
        public bool HasSpfRecord { get; set; }
        public bool HasDmarcRecord { get; set; }
        public bool HasDkimRecord { get; set; }
        public List<string> Findings { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
