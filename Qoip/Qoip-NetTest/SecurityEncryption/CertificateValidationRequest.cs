using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using System;
using System.Collections.Generic;
using System.Formats.Asn1;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    public class CertificateValidationRequest
    {
        public DetailLevel DetailLevel { get; } = DetailLevel.Ok;

        public string Url { get; }
        public int ExpirationWarningThresholdInDays { get; }
        public CertificateValidationRequest(string url, int expirationWarningThresholdInDays = 0)
        {
            Url = url;
            ExpirationWarningThresholdInDays = expirationWarningThresholdInDays;
        }

        public Response<CertificateValidationResponse> Execute()
        {
            using (var httpClientHandler = new HttpClientHandler())
            {
                CertificateValidationResponse certificateDetails = null;
                string errorMessage = null;
                ResponseStatus responseStatus = ResponseStatus.Ok;

                httpClientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) =>
                {
                    certificateDetails = ParseCertificate(cert);

                    // Set chain policy for revocation check
                    chain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                    chain.ChainPolicy.RevocationFlag = X509RevocationFlag.EntireChain;

                    bool isValid = chain.Build(cert);
                    if (isValid && sslPolicyErrors == SslPolicyErrors.None)
                    {
                        // Check if the certificate is expiring soon
                        if (ExpirationWarningThresholdInDays > 0 && cert.NotAfter <= DateTime.Now.AddDays(ExpirationWarningThresholdInDays))
                        {
                            responseStatus = ResponseStatus.Warning;
                            errorMessage = $"Certificate is expiring on {cert.NotAfter:yyyy-MM-dd}. It is within the threshold of {ExpirationWarningThresholdInDays} days.";
                        }
                        return true; // No SSL policy errors and certificate is valid
                    }
                    else
                    {
                        foreach (var status in chain.ChainStatus)
                        {
                            if (status.Status == X509ChainStatusFlags.Revoked)
                            {
                                errorMessage = "Certificate is revoked.";
                                break;
                            }
                            else if (status.Status == X509ChainStatusFlags.NotTimeValid)
                            {
                                errorMessage = "Certificate is expired.";
                                break;
                            }
                            else if (status.Status == X509ChainStatusFlags.UntrustedRoot)
                            {
                                errorMessage = "Certificate is untrusted.";
                                break;
                            }
                        }

                        if (errorMessage == null)
                        {
                            errorMessage = $"Certificate validation failed: {sslPolicyErrors}";
                        }

                        responseStatus = ResponseStatus.Failure;
                        return false;
                    }
                };

                using var httpClient = new HttpClient(httpClientHandler);
                try
                {
                    var response = httpClient.GetAsync(Url).Result;
                    if (response.IsSuccessStatusCode && certificateDetails != null)
                    {
                        string message = responseStatus == ResponseStatus.Warning
                            ? errorMessage
                            : "No SSL policy errors and certificate is valid.";
                        return new Response<CertificateValidationResponse>(responseStatus, certificateDetails, message);
                    }
                    else if (certificateDetails != null)
                    {
                        return new Response<CertificateValidationResponse>(responseStatus, certificateDetails, responseStatus == ResponseStatus.Warning ? errorMessage : "No SSL policy errors and certificate is valid.");
                    }
                    else
                    {
                        return new Response<CertificateValidationResponse>(ResponseStatus.Failure, certificateDetails, errorMessage ?? "Certificate validation failed.");
                    }
                }
                catch (HttpRequestException ex)
                {
                    return new Response<CertificateValidationResponse>(ResponseStatus.Failure, certificateDetails, errorMessage ?? $"HTTP request error: {ex.Message}");
                }
                catch (TaskCanceledException ex)
                {
                    return new Response<CertificateValidationResponse>(ResponseStatus.Failure, certificateDetails, errorMessage ?? $"Request timed out: {ex.Message}");
                }
                catch (Exception ex)
                {
                    return new Response<CertificateValidationResponse>(ResponseStatus.Failure, certificateDetails, errorMessage ?? $"An error occurred: {ex.Message} {(ex.InnerException != null ? $"Inner exception: {ex.InnerException.Message}" : string.Empty)}");
                }
            }
        }


        private CertificateValidationResponse ParseCertificate(X509Certificate2 cert)
        {
            var extensionData = new Dictionary<string, List<string>>();
            var alternativeNames = new List<string>();

            foreach (var extension in cert.Extensions)
            {
                string key = extension.Oid.FriendlyName ?? extension.Oid.Value;
                List<string> values;

                switch (extension.Oid.Value)
                {
                    case "2.5.29.17": // OID for Subject Alternative Name
                        values = ParseSubjectAlternativeNameExtension(extension, alternativeNames);
                        break;
                    case "2.5.29.15": // OID for Key Usage
                        values = ParseKeyUsageExtension(extension);
                        break;
                    case "2.5.29.19": // OID for Basic Constraints
                        values = ParseBasicConstraintsExtension(extension);
                        break;
                    case "2.5.29.37": // OID for Enhanced Key Usage
                        values = ParseEnhancedKeyUsageExtension(extension);
                        break;
                    case "2.5.29.32": // OID for Certificate Policies
                        values = ParseCertificatePoliciesExtension(extension);
                        break;
                    case "2.5.29.31": // OID for CRL Distribution Points
                        values = ParseCrlDistributionPointsExtension(extension);
                        break;
                    case "2.5.29.35": // OID for Authority Key Identifier
                        values = ParseAuthorityKeyIdentifierExtension(extension);
                        break;
                    case "2.5.29.14": // OID for Subject Key Identifier
                        values = ParseSubjectKeyIdentifierExtension(extension);
                        break;
                    case "1.3.6.1.5.5.7.1.1": // OID for Authority Information Access
                        values = ParseAuthorityInformationAccessExtension(extension);
                        break;
                    case "1.3.6.1.4.1.11129.2.4.2": // OID for SCT List
                        values = ParseSctListExtension(extension);
                        break;
                    default:
                        values = new List<string> { TryDecodeExtensionData(extension.RawData) };
                        break;
                }

                if (extensionData.ContainsKey(key))
                {
                    extensionData[key].AddRange(values);
                }
                else
                {
                    extensionData[key] = values;
                }
            }

            return new CertificateValidationResponse
            {
                IssuedTo = cert.Subject,
                IssuedBy = cert.Issuer,
                ValidityPeriod = $"{cert.NotBefore} - {cert.NotAfter}",
                Fingerprints = cert.Thumbprint,
                Version = cert.Version,
                Algorithm = cert.SignatureAlgorithm.FriendlyName,
                Extensions = extensionData,
                AlternativeNames = alternativeNames,
                ValidFrom = cert.NotBefore,
                ValidTo = cert.NotAfter
            };
        }

        private List<string> ParseSctListExtension(X509Extension extension)
        {
            List<string> sctList = new List<string>();

            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension), "Extension cannot be null.");
            }

            byte[] extensionValue = extension.RawData;

            try
            {
                bool isCritical = (extensionValue[0] & 0x01) == 0x01;
                sctList.Add($"Is Critical: {isCritical}");

                byte[] fieldValue = new byte[extensionValue.Length - 1];
                Array.Copy(extensionValue, 1, fieldValue, 0, fieldValue.Length);

                string fieldValueHex = BitConverter.ToString(fieldValue).Replace("-", "");
                sctList.Add($"Field Value: {fieldValueHex}");
            }
            catch (Exception ex)
            {
                sctList.Add($"Error parsing SCT list extension: {ex.Message}");
            }

            return sctList;
        }

        private List<string> ParseCertificatePoliciesExtension(X509Extension extension)
        {
            var policies = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var sequence = reader.ReadSequence();

                while (sequence.HasData)
                {
                    var policySequence = sequence.ReadSequence();
                    var policyIdentifier = policySequence.ReadObjectIdentifier();
                    policies.Add($"Policy Identifier: {policyIdentifier}");

                    if (policySequence.HasData)
                    {
                        var policyQualifiers = policySequence.ReadSequence();
                        while (policyQualifiers.HasData)
                        {
                            var policyQualifier = policyQualifiers.ReadSequence();
                            var qualifierId = policyQualifier.ReadObjectIdentifier();
                            var qualifierValue = policyQualifier.ReadCharacterString(UniversalTagNumber.IA5String);
                            policies.Add($"Qualifier ID: {qualifierId}, Qualifier Value: {qualifierValue}");
                        }
                    }
                }
            }
            catch (AsnContentException ex)
            {
                policies.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return policies;
        }

        private List<string> ParseCrlDistributionPointsExtension(X509Extension extension)
        {
            var crlDistributionPoints = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var sequence = reader.ReadSequence();

                while (sequence.HasData)
                {
                    var distributionPointSequence = sequence.ReadSequence();
                    if (distributionPointSequence.HasData)
                    {
                        var tag = distributionPointSequence.PeekTag();
                        if (tag.HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 0)))
                        {
                            var fullName = distributionPointSequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 0));
                            while (fullName.HasData)
                            {
                                var nameTag = fullName.PeekTag();
                                if (nameTag.HasSameClassAndValue(new Asn1Tag(UniversalTagNumber.IA5String)))
                                {
                                    var uri = fullName.ReadCharacterString(UniversalTagNumber.IA5String);
                                    crlDistributionPoints.Add(uri);
                                }
                                else
                                {
                                    fullName.ReadEncodedValue(); // Skip unexpected tag
                                }
                            }
                        }
                        else
                        {
                            distributionPointSequence.ReadEncodedValue(); // Skip unexpected tag
                        }
                    }
                }
            }
            catch (AsnContentException ex)
            {
                crlDistributionPoints.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return crlDistributionPoints;
        }

        private List<string> ParseSubjectAlternativeNameExtension(X509Extension extension, List<string> alternativeNames)
        {
            var sanList = new List<string>();
            var sanExtension = new X509SubjectAlternativeNameExtension(extension.RawData, false);

            foreach (var dnsName in sanExtension.EnumerateDnsNames())
            {
                sanList.Add($"DNS Name: {dnsName}");
                alternativeNames.Add(dnsName);
            }

            return sanList;
        }

        private List<string> ParseKeyUsageExtension(X509Extension extension)
        {
            var keyUsages = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var keyUsageBits = reader.ReadNamedBitList();
                var bitArray = new System.Collections.BitArray(keyUsageBits);

                if (bitArray.Length > 0 && bitArray[0]) keyUsages.Add("Digital Signature");
                if (bitArray.Length > 1 && bitArray[1]) keyUsages.Add("Non Repudiation");
                if (bitArray.Length > 2 && bitArray[2]) keyUsages.Add("Key Encipherment");
                if (bitArray.Length > 3 && bitArray[3]) keyUsages.Add("Data Encipherment");
                if (bitArray.Length > 4 && bitArray[4]) keyUsages.Add("Key Agreement");
                if (bitArray.Length > 5 && bitArray[5]) keyUsages.Add("Key Cert Sign");
                if (bitArray.Length > 6 && bitArray[6]) keyUsages.Add("CRL Sign");
                if (bitArray.Length > 7 && bitArray[7]) keyUsages.Add("Encipher Only");
                if (bitArray.Length > 8 && bitArray[8]) keyUsages.Add("Decipher Only");
            }
            catch (AsnContentException ex)
            {
                keyUsages.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return keyUsages;
        }

        private List<string> ParseBasicConstraintsExtension(X509Extension extension)
        {
            var basicConstraints = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var sequence = reader.ReadSequence();

                bool isCA = false;
                if (sequence.HasData)
                {
                    var tag = sequence.PeekTag();
                    if (tag.TagValue == (int)UniversalTagNumber.Boolean)
                    {
                        isCA = sequence.ReadBoolean();
                    }
                }
                basicConstraints.Add($"Certificate Authority: {isCA}");

                if (sequence.HasData)
                {
                    var tag = sequence.PeekTag();
                    if (tag.TagValue == (int)UniversalTagNumber.Integer)
                    {
                        var pathLengthConstraint = sequence.ReadInteger();
                        basicConstraints.Add($"Path Length Constraint: {pathLengthConstraint}");
                    }
                }
            }
            catch (AsnContentException ex)
            {
                basicConstraints.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return basicConstraints;
        }

        private List<string> ParseEnhancedKeyUsageExtension(X509Extension extension)
        {
            var ekuList = new List<string>();
            var enhancedKeyUsageExtension = (X509EnhancedKeyUsageExtension)extension;

            foreach (var oid in enhancedKeyUsageExtension.EnhancedKeyUsages)
            {
                ekuList.Add(oid.FriendlyName ?? oid.Value);
            }

            return ekuList;
        }

        private List<string> ParseAuthorityKeyIdentifierExtension(X509Extension extension)
        {
            var authorityKeyIdentifiers = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var sequence = reader.ReadSequence();

                while (sequence.HasData)
                {
                    var tag = sequence.PeekTag();
                    if (tag.HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 0)))
                    {
                        var keyIdentifier = sequence.ReadOctetString(new Asn1Tag(TagClass.ContextSpecific, 0));
                        authorityKeyIdentifiers.Add($"Key Identifier: {BitConverter.ToString(keyIdentifier)}");
                    }
                    else if (tag.HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 1)))
                    {
                        var issuerSequence = sequence.ReadSequence(new Asn1Tag(TagClass.ContextSpecific, 1));
                        var issuer = issuerSequence.ReadCharacterString(UniversalTagNumber.IA5String);
                        authorityKeyIdentifiers.Add($"Authority Cert Issuer: {issuer}");
                    }
                    else if (tag.HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 2)))
                    {
                        var serialNumber = sequence.ReadInteger(new Asn1Tag(TagClass.ContextSpecific, 2));
                        authorityKeyIdentifiers.Add($"Authority Cert Serial Number: {serialNumber}");
                    }
                    else
                    {
                        throw new AsnContentException($"Unexpected tag: {tag}");
                    }
                }
            }
            catch (AsnContentException ex)
            {
                authorityKeyIdentifiers.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return authorityKeyIdentifiers;
        }

        private List<string> ParseAuthorityInformationAccessExtension(X509Extension extension)
        {
            var authorityInformationAccess = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var sequence = reader.ReadSequence();

                while (sequence.HasData)
                {
                    var accessDescriptionSequence = sequence.ReadSequence();
                    var accessMethod = accessDescriptionSequence.ReadObjectIdentifier();
                    var tag = accessDescriptionSequence.PeekTag();
                    if (tag.HasSameClassAndValue(new Asn1Tag(TagClass.ContextSpecific, 6)))
                    {
                        var accessLocation = accessDescriptionSequence.ReadCharacterString(UniversalTagNumber.IA5String, new Asn1Tag(TagClass.ContextSpecific, 6));
                        authorityInformationAccess.Add($"Access Method: {accessMethod}, Access Location: {accessLocation}");
                    }
                }
            }
            catch (AsnContentException ex)
            {
                authorityInformationAccess.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return authorityInformationAccess;
        }

        private List<string> ParseSubjectKeyIdentifierExtension(X509Extension extension)
        {
            var subjectKeyIdentifiers = new List<string>();
            var rawData = extension.RawData;

            try
            {
                var reader = new AsnReader(rawData, AsnEncodingRules.DER);
                var keyIdentifier = reader.ReadOctetString();
                subjectKeyIdentifiers.Add(BitConverter.ToString(keyIdentifier));
            }
            catch (AsnContentException ex)
            {
                subjectKeyIdentifiers.Add($"Error parsing ASN.1 data: {ex.Message}");
            }

            return subjectKeyIdentifiers;
        }

        private string TryDecodeExtensionData(byte[] rawData)
        {
            if (IsValidEncoding(rawData, Encoding.UTF8))
            {
                return Encoding.UTF8.GetString(rawData);
            }

            if (IsValidEncoding(rawData, Encoding.ASCII))
            {
                return Encoding.ASCII.GetString(rawData);
            }

            return BitConverter.ToString(rawData);
        }

        private bool IsValidEncoding(byte[] data, Encoding encoding)
        {
            try
            {
                string decodedString = encoding.GetString(data);
                if (decodedString.Any(c => (c < 32 && c != 9 && c != 10 && c != 13) || c > 126))
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
