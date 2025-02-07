using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class ARecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            var ip = new IPAddress(new[] { response[offset], response[offset + 1], response[offset + 2], response[offset + 3] });
            offset += dataLength;
            return new[] { ip.ToString() };
        }
    }
}
