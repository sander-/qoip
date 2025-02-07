using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class CaaRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
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

            return new[] { $"Flags: {flags}, Tag: {tag}, Value: {value}" };
        }
    }
}
