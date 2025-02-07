using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class TxtRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            var txtLength = response[offset++];
            if (offset + txtLength > response.Length) // Ensure there's enough data for the text
            {
                throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
            }
            var txt = Encoding.ASCII.GetString(response, offset, txtLength);
            offset += txtLength;
            return new[] { txt };
        }
    }
}
