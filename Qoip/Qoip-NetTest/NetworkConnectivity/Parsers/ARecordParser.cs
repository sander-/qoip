using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class ARecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            var ipAddresses = new List<string>();

            while (dataLength >= 4)
            {
                var ip = new IPAddress(new[] { response[offset], response[offset + 1], response[offset + 2], response[offset + 3] });
                ipAddresses.Add(ip.ToString());
                offset += 4;
                dataLength -= 4;
            }

            // Add each A record to the dictionary of additional details
            foreach (var ip in ipAddresses)
            {
                var key = "A Record";
                var count = 1;
                while (additionalDetails.ContainsKey($"{key} {count}"))
                {
                    count++;
                }
                additionalDetails[$"{key} {count}"] = ip;
            }

            return ipAddresses;
        }
    }
}
