using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class SpfRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            if (dataLength < 1) // Ensure there's enough data for the SPF record
            {
                throw new IndexOutOfRangeException("Invalid data length for SPF record.");
            }

            var spf = Encoding.ASCII.GetString(response, offset, dataLength);
            offset += dataLength;

            // Add the SPF record to the dictionary of additional details
            additionalDetails["SPF Record"] = spf;

            return new[] { spf };
        }
    }
}



