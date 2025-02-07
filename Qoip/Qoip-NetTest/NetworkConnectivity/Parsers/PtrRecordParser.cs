using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class PtrRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            var ptr = new StringBuilder();
            var length = response[offset++];
            while (length != 0)
            {
                if (offset + length > response.Length) // Ensure there's enough data for the label
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }
                ptr.Append(Encoding.ASCII.GetString(response, offset, length));
                offset += length;
                length = response[offset++];
                if (length != 0)
                {
                    ptr.Append(".");
                }
            }
            return new[] { ptr.ToString() };
        }
    }
}
