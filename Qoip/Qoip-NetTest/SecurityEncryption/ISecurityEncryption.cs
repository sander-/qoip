using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.SecurityEncryption;

namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    // Security & Encryption
    public interface ISecurityEncryption
    {
        Response<CertificateValidationResponse> ValidateTLS(string url);
        
    }
}
