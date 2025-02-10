using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.SecurityEncryption;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    // Security & Encryption
    public interface ISecurityEncryption
    {
        Response<CertificateValidationResponse> ValidateCertificate(string url, int expirationWarningThresholdInDays = 0);
        ISecurityEncryption ValidateCertificates(string certificatePath);
        ISecurityEncryption ValidateEncryptedCommunication(string message);
    }
}
