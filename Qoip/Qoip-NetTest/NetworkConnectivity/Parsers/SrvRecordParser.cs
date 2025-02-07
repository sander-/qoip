using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class SrvRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            if (dataLength < 6) // Ensure there's enough data for the SRV record
            {
                throw new IndexOutOfRangeException("Invalid data length for SRV record.");
            }

            var priority = (response[offset] << 8) | response[offset + 1];
            var weight = (response[offset + 2] << 8) | response[offset + 3];
            var port = (response[offset + 4] << 8) | response[offset + 5];
            offset += 6;

            var target = new StringBuilder();
            var length = response[offset++];
            while (length != 0)
            {
                if (offset + length > response.Length) // Ensure there's enough data for the label
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }
                target.Append(Encoding.ASCII.GetString(response, offset, length));
                offset += length;
                length = response[offset++];
                if (length != 0)
                {
                    target.Append(".");
                }
            }

            return new[] { $"Priority: {priority}, Weight: {weight}, Port: {port}, Target: {target}" };
        }
    }
}
