using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class TxtRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            var txtRecords = new List<string>();
            var endOffset = offset + dataLength;

            while (offset < endOffset)
            {
                var txtLength = response[offset++];
                if (offset + txtLength > response.Length) // Ensure there's enough data for the text
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }
                var txt = Encoding.ASCII.GetString(response, offset, txtLength);
                offset += txtLength;
                txtRecords.Add(txt);
            }

            // Add the TXT records to the dictionary of additional details
            for (int i = 0; i < txtRecords.Count; i++)
            {
                additionalDetails[$"TXT Record {i + 1}"] = txtRecords[i];
            }

            return txtRecords;
        }
    }
}

