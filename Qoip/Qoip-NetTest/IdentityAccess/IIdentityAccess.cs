namespace Qoip.ZeroTrustNetwork.IdentityAccess
{
    // Identity & Access
    public interface IIdentityAccess
    {
        IIdentityAccess VerifyAuthentication(string username, string password);
        IIdentityAccess VerifyAuthorization(string username, string resource);
    }
}
