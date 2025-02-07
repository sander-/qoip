using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class MxRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            try
            {
                if (dataLength < 3) // Ensure there's enough data for the MX record
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }

                var preference = (response[offset] << 8) | response[offset + 1];
                offset += 2; // Skip preference

                var exchange = ReadDomainName(response, ref offset);

                // Add the MX record to the dictionary of additional details
                additionalDetails[exchange] = preference.ToString();

                return new[] { exchange };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing MX record: {ex.Message}", ex);
            }
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

