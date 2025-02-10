using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using Qoip.ZeroTrustNetwork.SecurityEncryption;
using System.Security.Cryptography.X509Certificates;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    public class SecurityEncryption : ISecurityEncryption
    {
        private Response<CertificateValidationResponse> _certificateValidationResponse;


        public SecurityEncryption WithCertificateAt(string url)
        {
            _certificateValidationResponse = GetCertificateFromUrl(url);
            return this;
        }

        public DateTime GetExpiration()
        {
            return _certificateValidationResponse.Data.ValidTo;
        }

        public Response<CertificateValidationResponse> ValidateCertificate(string url, int expirationWarningThresholdInDays = 0)
        {
            var request = new CertificateValidationRequest(url, expirationWarningThresholdInDays);
            _certificateValidationResponse = request.Execute();
            return _certificateValidationResponse;
        }

        public ISecurityEncryption ValidateCertificates(string certificatePath)
        {
            // Implement certificate validation logic here
            // For example, load and validate certificates from the given path
            return this;
        }

        public ISecurityEncryption ValidateEncryptedCommunication(string message)
        {
            // Implement encrypted communication validation logic here
            // For example, decrypt and validate the message
            return this;
        }

        private Response<CertificateValidationResponse> GetCertificateFromUrl(string url)
        {
            var request = new CertificateValidationRequest(url);
            return request.Execute();
        }

        public Response<CertificateValidationResponse> GetValidationResponse()
        {
            return _certificateValidationResponse;
        }
    }
}
