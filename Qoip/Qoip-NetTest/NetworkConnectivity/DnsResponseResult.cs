using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class DnsResponseResult
    {
        public string[] Records { get; set; }
        public bool IsAuthoritative { get; set; }
        public int Ttl { get; set; }
        public Dictionary<string, string> AdditionalDetails { get; set; }

        public DnsResponseResult(string[] records, bool isAuthoritative, int ttl, Dictionary<string, string> additionalDetails)
        {
            Records = records;
            IsAuthoritative = isAuthoritative;
            Ttl = ttl;
            AdditionalDetails = additionalDetails;
        }
    }
}
