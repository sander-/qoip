using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class DnsResponse
    {
        public string? Server { get; set; }
        public string? Address { get; set; }
        public string? QueryType { get; set; }
        public List<string> Records { get; set; }
        public bool IsAuthoritative { get; set; }
        public int TTL { get; set; }
        public Dictionary<string, string> AdditionalDetails { get; set; }

        public DnsResponse()
        {
            Records = new List<string>();
            AdditionalDetails = new Dictionary<string, string>();
        }

        public string FirstRecord
        {
            get
            {
                if (Records == null || !Records.Any())
                {
                    throw new InvalidOperationException("No records available.");
                }
                return Records.First();
            }
        }

        public override string ToString()
        {
            var jsonData = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            return jsonData;
        }
    }
}
