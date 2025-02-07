using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Qoip_NetTest.NetworkConnectivity.Parsers
{
    public class NsecRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength)
        {
            var nextDomainName = ReadDomainName(response, ref offset);
            var typeBitMaps = new List<string>();

            while (offset < response.Length)
            {
                var windowBlock = response[offset++];
                var bitmapLength = response[offset++];
                var bitmap = response.Skip(offset).Take(bitmapLength).ToArray();
                offset += bitmapLength;

                for (int i = 0; i < bitmap.Length; i++)
                {
                    for (int bit = 0; bit < 8; bit++)
                    {
                        if ((bitmap[i] & (1 << (7 - bit))) != 0)
                        {
                            var typeCode = (windowBlock << 8) + (i * 8) + bit;
                            typeBitMaps.Add(typeCode.ToString());
                        }
                    }
                }
            }

            return new[] { $"NextDomainName: {nextDomainName}, TypeBitMaps: {string.Join(", ", typeBitMaps)}" };
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
