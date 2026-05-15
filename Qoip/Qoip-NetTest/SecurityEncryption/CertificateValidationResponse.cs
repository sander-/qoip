using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    public class CertificateValidationResponse
    {
        public string IssuedTo { get; set; } = string.Empty;
        public string IssuedBy { get; set; } = string.Empty;
        public string ValidityPeriod { get; set; } = string.Empty;
        public string Fingerprints { get; set; } = string.Empty;
        public int Version { get; set; }
        public string Algorithm { get; set; } = string.Empty;
        public string Usage { get; set; } = string.Empty;
        public List<string> AlternativeNames { get; set; } = new();
        public Dictionary<string, List<string>> Extensions { get; set; } = new();
        public DateTime ValidFrom { get; set; } 
        public DateTime ValidTo { get; set; }   

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}
