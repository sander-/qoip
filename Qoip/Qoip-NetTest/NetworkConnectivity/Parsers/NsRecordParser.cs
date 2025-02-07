using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class NsRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            var nsRecords = new List<string>();

            while (dataLength > 0)
            {
                var ns = new StringBuilder();
                int length = response[offset++];

                while (length != 0)
                {
                    if ((length & 0xC0) == 0xC0) // Handle DNS name compression
                    {
                        int pointer = ((length & 0x3F) << 8) | response[offset++];
                        ParseCompressedName(response, ref pointer, ns);
                        break;
                    }
                    else
                    {
                        if (offset + length > response.Length) // Ensure there's enough data for the label
                        {
                            throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                        }
                        ns.Append(Encoding.ASCII.GetString(response, offset, length));
                        offset += length;
                        length = response[offset++];
                        if (length != 0)
                        {
                            ns.Append(".");
                        }
                    }
                }

                // Add the NS record to the list
                nsRecords.Add(ns.ToString());

                // Add the NS record to the dictionary of additional details
                var key = "NS Record";
                var count = 1;
                while (additionalDetails.ContainsKey($"{key} {count}"))
                {
                    count++;
                }
                additionalDetails[$"{key} {count}"] = ns.ToString();

                // Skip the rest of the RDATA
                offset += dataLength - (ns.Length + 2);
                dataLength -= (ns.Length + 2);
            }

            return nsRecords;
        }

        private void ParseCompressedName(byte[] response, ref int offset, StringBuilder ns)
        {
            int length = response[offset++];
            while (length != 0)
            {
                if ((length & 0xC0) == 0xC0) // Handle nested compression
                {
                    int pointer = ((length & 0x3F) << 8) | response[offset++];
                    ParseCompressedName(response, ref pointer, ns);
                    return;
                }
                else
                {
                    if (offset + length > response.Length) // Ensure there's enough data for the label
                    {
                        throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                    }
                    ns.Append(Encoding.ASCII.GetString(response, offset, length));
                    offset += length;
                    length = response[offset++];
                    if (length != 0)
                    {
                        ns.Append(".");
                    }
                }
            }
        }
    }
}
