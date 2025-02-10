using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    public class CertificateValidationResponse
    {
        public string IssuedTo { get; set; }
        public string IssuedBy { get; set; }
        public string ValidityPeriod { get; set; }
        public string Fingerprints { get; set; }
        public int Version { get; set; }
        public string Algorithm { get; set; }
        public string Usage { get; set; }
        public List<string> AlternativeNames { get; set; }
        public Dictionary<string, List<string>> Extensions { get; set; }

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}
