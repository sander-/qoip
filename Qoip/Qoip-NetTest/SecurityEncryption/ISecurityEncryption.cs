namespace Qoip.ZeroTrustNetwork.SecurityEncryption
{
    // Security & Encryption
    public interface ISecurityEncryption
    {
        ISecurityEncryption ValidateTLS(string url);
        ISecurityEncryption ValidateCertificates(string certificatePath);
        ISecurityEncryption ValidateEncryptedCommunication(string message);
    }
}
