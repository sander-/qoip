using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.SecurityEncryption;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    public class SecurityEncryption : ISecurityEncryption
    {
        private Response<CertificateValidationResponse>? _certificateValidationResponse;


        public SecurityEncryption WithCertificateAt(string url)
        {
            ValidateCertificate(url);
            return this;
        }

        public DateTime GetExpiration()
        {
            return _certificateValidationResponse?.Data?.ValidTo ?? throw new InvalidOperationException("Certificate has not been validated.");
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

        public Response<CertificateValidationResponse> GetValidationResponse()
        {
            return _certificateValidationResponse ?? new Response<CertificateValidationResponse>(ResponseStatus.Failure, null, "Certificate has not been validated.");
        }
    }
}
