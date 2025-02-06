using System.Collections.Generic;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.Common
{
    public class DnsLookupResponse
    {
        public string Server { get; set; }
        public string Address { get; set; }
        public string QueryType { get; set; }
        public List<string> Addresses { get; set; }

        public DnsLookupResponse()
        {
            Addresses = new List<string>();
        }

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}




