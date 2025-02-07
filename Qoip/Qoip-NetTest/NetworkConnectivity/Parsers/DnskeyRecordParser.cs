using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class DnskeyRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
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

            // Add the DNSKEY record details to the dictionary of additional details
            additionalDetails["Flags"] = flags.ToString();
            additionalDetails["Protocol"] = protocol.ToString();
            additionalDetails["Algorithm"] = algorithm.ToString();
            additionalDetails["PublicKey"] = publicKey;

            return new[] { $"Flags: {flags}, Protocol: {protocol}, Algorithm: {algorithm}, PublicKey: {publicKey}" };
        }
    }
}







