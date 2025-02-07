using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class SoaRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            var mname = ReadDomainName(response, ref offset);
            var rname = ReadDomainName(response, ref offset);

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

            return new[] { $"MNAME: {mname}, RNAME: {rname}, Serial: {serial}, Refresh: {refresh}, Retry: {retry}, Expire: {expire}, Minimum: {minimum}" };
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
