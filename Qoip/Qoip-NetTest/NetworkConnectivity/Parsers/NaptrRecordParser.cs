using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class NaptrRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            if (dataLength < 13) // Ensure there's enough data for the NAPTR record
            {
                throw new IndexOutOfRangeException("Invalid data length for NAPTR record.");
            }

            var order = (response[offset] << 8) | response[offset + 1];
            var preference = (response[offset + 2] << 8) | response[offset + 3];
            offset += 4;

            var flags = ReadString(response, ref offset);
            var services = ReadString(response, ref offset);
            var regexp = ReadString(response, ref offset);
            var replacement = ReadDomainName(response, ref offset);

            // Add the NAPTR record details to the dictionary of additional details
            additionalDetails["Order"] = order.ToString();
            additionalDetails["Preference"] = preference.ToString();
            additionalDetails["Flags"] = flags;
            additionalDetails["Services"] = services;
            additionalDetails["Regexp"] = regexp;
            additionalDetails["Replacement"] = replacement;

            return new[] { $"Order: {order}, Preference: {preference}, Flags: {flags}, Services: {services}, Regexp: {regexp}, Replacement: {replacement}" };
        }

        private string ReadString(byte[] response, ref int offset)
        {
            var length = response[offset++];
            var value = Encoding.ASCII.GetString(response, offset, length);
            offset += length;
            return value;
        }

        private string ReadDomainName(byte[] response, ref int offset)
        {
            var domainName = new StringBuilder();
            var length = response[offset++];
            while (length != 0)
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
            return domainName.ToString();
        }
    }
}






