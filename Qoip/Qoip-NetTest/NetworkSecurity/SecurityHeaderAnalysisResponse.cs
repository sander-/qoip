using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkSecurity
{
    public class SecurityHeaderAnalysisResponse
    {
        public string Url { get; set; } = string.Empty;
        public DetailLevel DetailLevel { get; set; }
        public bool HasStrictTransportSecurity { get; set; }
        public bool HasContentSecurityPolicy { get; set; }
        public bool HasXFrameOptions { get; set; }
        public bool HasXContentTypeOptions { get; set; }
        public bool HasReferrerPolicy { get; set; }
        public bool HasPermissionsPolicy { get; set; }
        public Dictionary<string, string> PresentHeaders { get; set; } = new();
        public List<string> MissingHeaders { get; set; } = new();
        public List<string> Findings { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
