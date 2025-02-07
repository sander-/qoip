using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class TlsaRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            if (dataLength < 4) // Ensure there's enough data for the TLSA record
            {
                throw new IndexOutOfRangeException("Invalid data length for TLSA record.");
            }

            var usage = response[offset];
            var selector = response[offset + 1];
            var matchingType = response[offset + 2];
            offset += 3;

            var certificateAssociationData = BitConverter.ToString(response, offset, dataLength - 3).Replace("-", "");
            offset += dataLength - 3;

            return new[] { $"Usage: {usage}, Selector: {selector}, MatchingType: {matchingType}, CertificateAssociationData: {certificateAssociationData}" };
        }
    }
}
