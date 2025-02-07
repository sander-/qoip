using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class DnskeyRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            if (dataLength < 4) // Ensure there's enough data for the DNSKEY record
            {
                throw new IndexOutOfRangeException("Invalid data length for DNSKEY record.");
            }

            var flags = (response[offset] << 8) | response[offset + 1];
            var protocol = response[offset + 2];
            var algorithm = response[offset + 3];
            offset += 4;

            var publicKey = Convert.ToBase64String(response, offset, dataLength - 4);
            offset += dataLength - 4;

            return new[] { $"Flags: {flags}, Protocol: {protocol}, Algorithm: {algorithm}, PublicKey: {publicKey}" };
        }
    }
}
