using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class Nsec3RecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            if (dataLength < 5) // Ensure there's enough data for the NSEC3 record
            {
                throw new IndexOutOfRangeException("Invalid data length for NSEC3 record.");
            }

            var hashAlgorithm = response[offset];
            var flags = response[offset + 1];
            var iterations = (response[offset + 2] << 8) | response[offset + 3];
            var saltLength = response[offset + 4];
            offset += 5;

            var salt = BitConverter.ToString(response, offset, saltLength).Replace("-", "");
            offset += saltLength;

            var hashLength = response[offset];
            offset += 1;

            var nextHashedOwnerName = BitConverter.ToString(response, offset, hashLength).Replace("-", "");
            offset += hashLength;

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

            // Add the NSEC3 record details to the dictionary of additional details
            additionalDetails["HashAlgorithm"] = hashAlgorithm.ToString();
            additionalDetails["Flags"] = flags.ToString();
            additionalDetails["Iterations"] = iterations.ToString();
            additionalDetails["Salt"] = salt;
            additionalDetails["NextHashedOwnerName"] = nextHashedOwnerName;
            additionalDetails["TypeBitMaps"] = string.Join(", ", typeBitMaps);

            return new[] { $"HashAlgorithm: {hashAlgorithm}, Flags: {flags}, Iterations: {iterations}, Salt: {salt}, NextHashedOwnerName: {nextHashedOwnerName}, TypeBitMaps: {string.Join(", ", typeBitMaps)}" };
        }
    }
}





