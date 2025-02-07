using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class CaaRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            if (dataLength < 5) // Ensure there's enough data for the CAA record
            {
                throw new IndexOutOfRangeException("Invalid data length for CAA record.");
            }

            var flags = response[offset];
            offset += 1;

            var tagLength = response[offset];
            offset += 1;

            var tag = Encoding.ASCII.GetString(response, offset, tagLength);
            offset += tagLength;

            var valueLength = dataLength - 2 - tagLength;
            var value = Encoding.ASCII.GetString(response, offset, valueLength);
            offset += valueLength;

            // Add the CAA record details to the dictionary of additional details
            additionalDetails["Flags"] = flags.ToString();
            additionalDetails["Tag"] = tag;
            additionalDetails["Value"] = value;

            return new[] { $"Flags: {flags}, Tag: {tag}, Value: {value}" };
        }
    }
}










