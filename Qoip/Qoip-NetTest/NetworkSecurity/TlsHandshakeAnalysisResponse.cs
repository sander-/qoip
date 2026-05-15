using Qoip.ZeroTrustNetwork.Common;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkSecurity
{
    public class TlsHandshakeAnalysisResponse
    {
        public string Host { get; set; } = string.Empty;
        public int Port { get; set; }
        public DetailLevel DetailLevel { get; set; }
        public bool HandshakeSucceeded { get; set; }
        public string NegotiatedSslProtocol { get; set; } = string.Empty;
        public string CipherAlgorithm { get; set; } = string.Empty;
        public int CipherStrength { get; set; }
        public string HashAlgorithm { get; set; } = string.Empty;
        public int HashStrength { get; set; }
        public string KeyExchangeAlgorithm { get; set; } = string.Empty;
        public int KeyExchangeStrength { get; set; }
        public string CertificateSubject { get; set; } = string.Empty;
        public string CertificateIssuer { get; set; } = string.Empty;
        public DateTime CertificateValidFrom { get; set; }
        public DateTime CertificateValidTo { get; set; }
        public List<string> Findings { get; set; } = new();

        public override string ToString()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}
