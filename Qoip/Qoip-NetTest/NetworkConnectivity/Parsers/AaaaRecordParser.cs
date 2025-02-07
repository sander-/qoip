using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class AaaaRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            if (dataLength != 16) // IPv6 addresses are 16 bytes long
            {
                throw new IndexOutOfRangeException("Invalid data length for AAAA record.");
            }

            var ip = new IPAddress(new[]
            {
                response[offset], response[offset + 1], response[offset + 2], response[offset + 3],
                response[offset + 4], response[offset + 5], response[offset + 6], response[offset + 7],
                response[offset + 8], response[offset + 9], response[offset + 10], response[offset + 11],
                response[offset + 12], response[offset + 13], response[offset + 14], response[offset + 15]
            });
            offset += dataLength;
            return new[] { ip.ToString() };
        }
    }
}
