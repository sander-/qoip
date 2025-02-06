using Qoip.ZeroTrustNetwork.Common;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    // Network Connectivity
    public interface INetworkConnectivity
    {
        Response<bool> CheckNetworkAccess(string ipAddress);
        Response<bool> CheckFirewallRules(string ipAddress);
        Response<bool> CheckNetworkSegmentation(string sourceIp, string destinationIp);
        public Response<DnsLookupResponse> PerformDnsLookup(string domainName, string dnsServer = null, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Ok, string queryType="A");
    }
}


