using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class SpfRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            if (dataLength < 1) // Ensure there's enough data for the SPF record
            {
                throw new IndexOutOfRangeException("Invalid data length for SPF record.");
            }

            var spf = Encoding.ASCII.GetString(response, offset, dataLength);
            offset += dataLength;

            return new[] { spf };
        }
    }
}
