using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class SoaRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            try
            {
                var mname = ReadDomainName(response, ref offset);
                var rname = ReadDomainName(response, ref offset);

                // Convert the first dot in rname to '@'
                var rnameParts = rname.Split(new[] { '.' }, 2);
                if (rnameParts.Length == 2)
                {
                    rname = $"{rnameParts[0]}@{rnameParts[1]}";
                }

                if (offset + 20 > response.Length) // Ensure there's enough data for the SOA record
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }

                var serial = BitConverter.ToUInt32(response.Skip(offset).Take(4).Reverse().ToArray(), 0);
                offset += 4;
                var refresh = BitConverter.ToInt32(response.Skip(offset).Take(4).Reverse().ToArray(), 0);
                offset += 4;
                var retry = BitConverter.ToInt32(response.Skip(offset).Take(4).Reverse().ToArray(), 0);
                offset += 4;
                var expire = BitConverter.ToInt32(response.Skip(offset).Take(4).Reverse().ToArray(), 0);
                offset += 4;
                var minimum = BitConverter.ToUInt32(response.Skip(offset).Take(4).Reverse().ToArray(), 0);
                offset += 4;

                additionalDetails["Server"] = mname;
                additionalDetails["Email"] = rname;
                additionalDetails["Serial"] = serial.ToString();
                additionalDetails["Refresh"] = refresh.ToString();
                additionalDetails["Retry"] = retry.ToString();
                additionalDetails["Expire"] = expire.ToString();
                additionalDetails["MinimumTTL"] = minimum.ToString();

                return new[] { mname };
            }
            catch (Exception ex)
            {
                throw new Exception($"Error parsing SOA record: {ex.Message}", ex);
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
