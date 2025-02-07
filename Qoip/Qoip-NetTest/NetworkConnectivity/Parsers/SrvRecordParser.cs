using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class SrvRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            if (dataLength < 6) // Ensure there's enough data for the SRV record
            {
                throw new IndexOutOfRangeException("Invalid data length for SRV record.");
            }

            var priority = (response[offset] << 8) | response[offset + 1];
            var weight = (response[offset + 2] << 8) | response[offset + 3];
            var port = (response[offset + 4] << 8) | response[offset + 5];
            offset += 6;

            var target = ReadDomainName(response, ref offset);

            // Add the SRV record details to the dictionary of additional details
            additionalDetails["Priority"] = priority.ToString();
            additionalDetails["Weight"] = weight.ToString();
            additionalDetails["Port"] = port.ToString();
            additionalDetails["Target"] = target;

            return new[] { $"Priority: {priority}, Weight: {weight}, Port: {port}, Target: {target}" };
        }

        private string ReadDomainName(byte[] response, ref int offset)
        {
            var domainName = new StringBuilder();
            var length = response[offset++];
            while (length != 0)
            {
                if ((length & 0xC0) == 0xC0) // Pointer
                {
                    var pointerOffset = ((length & 0x3F) << 8) | response[offset++];
                    var savedOffset = offset;
                    offset = pointerOffset;
                    domainName.Append(ReadDomainName(response, ref offset));
                    offset = savedOffset;
                    break;
                }
                else
                {
                    if (offset + length > response.Length) // Ensure there's enough data for the label
                    {
                        throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                    }
                    domainName.Append(Encoding.ASCII.GetString(response, offset, length));
                    offset += length;
                    length = response[offset++];
                    if (length != 0)
                    {
                        domainName.Append(".");
                    }
                }
            }
            return domainName.ToString();
        }
    }
}


