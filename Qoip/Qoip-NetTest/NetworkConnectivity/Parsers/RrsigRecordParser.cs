using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers
{
    public class RrsigRecordParser : IDnsResponseParser
    {
        public IEnumerable<string> Parse(byte[] response, ref int offset, int dataLength, Dictionary<string, string> additionalDetails)
        {
            if (dataLength < 18) // Ensure there's enough data for the RRSIG record
            {
                throw new IndexOutOfRangeException("Invalid data length for RRSIG record.");
            }

            var typeCovered = (response[offset] << 8) | response[offset + 1];
            var algorithm = response[offset + 2];
            var labels = response[offset + 3];
            var originalTtl = BitConverter.ToUInt32(response.Skip(offset + 4).Take(4).Reverse().ToArray(), 0);
            var signatureExpiration = BitConverter.ToUInt32(response.Skip(offset + 8).Take(4).Reverse().ToArray(), 0);
            var signatureInception = BitConverter.ToUInt32(response.Skip(offset + 12).Take(4).Reverse().ToArray(), 0);
            var keyTag = (response[offset + 16] << 8) | response[offset + 17];
            offset += 18;

            var signerName = ReadDomainName(response, ref offset);
            var signature = Convert.ToBase64String(response, offset, dataLength - (offset - 18));

            // Add the RRSIG record details to the dictionary of additional details
            additionalDetails["TypeCovered"] = typeCovered.ToString();
            additionalDetails["Algorithm"] = algorithm.ToString();
            additionalDetails["Labels"] = labels.ToString();
            additionalDetails["OriginalTTL"] = originalTtl.ToString();
            additionalDetails["SignatureExpiration"] = signatureExpiration.ToString();
            additionalDetails["SignatureInception"] = signatureInception.ToString();
            additionalDetails["KeyTag"] = keyTag.ToString();
            additionalDetails["SignerName"] = signerName;
            additionalDetails["Signature"] = signature;

            return new[] { $"TypeCovered: {typeCovered}, Algorithm: {algorithm}, Labels: {labels}, OriginalTTL: {originalTtl}, SignatureExpiration: {signatureExpiration}, SignatureInception: {signatureInception}, KeyTag: {keyTag}, SignerName: {signerName}, Signature: {signature}" };
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



