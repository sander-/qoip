using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.Common
{
    public class DnsLookupResponse
    {
        public string Server { get; set; }
        public string Address { get; set; }
        public string QueryType { get; set; }
        public List<string> Records { get; set; }
        public bool IsAuthoritative { get; set; }
        public int TTL { get; set; }
        public Dictionary<string, string> AdditionalDetails { get; set; }

        public DnsLookupResponse()
        {
            Records = new List<string>();
            AdditionalDetails = new Dictionary<string, string>();
        }

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}
