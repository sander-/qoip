using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public static class NetworkConnectivityExtensions
    {
        public static NetworkConnectivity WithDnsServer(this NetworkConnectivity networkConnectivity, string dnsServer)
        {
            networkConnectivity.DnsServer = dnsServer;
            return networkConnectivity;
        }

        public static NetworkConnectivity WithQueryType(this NetworkConnectivity networkConnectivity, string queryType)
        {
            networkConnectivity.QueryType = queryType;
            return networkConnectivity;
        }

        public static NetworkConnectivity WithDetailLevel(this NetworkConnectivity networkConnectivity, DetailLevel detailLevel)
        {
            networkConnectivity.DetailLevel = detailLevel;
            return networkConnectivity;
        }

        public static NetworkConnectivity WithTimeout(this NetworkConnectivity networkConnectivity, int timeout)
        {
            networkConnectivity.Timeout = timeout;
            return networkConnectivity;
        }

        public static Response<DnsResponse> ExecuteDnsRequest(this NetworkConnectivity networkConnectivity, string domainName)
        {
            return networkConnectivity.ExecuteDnsRequest(domainName, networkConnectivity.DnsServer, networkConnectivity.Timeout, networkConnectivity.DetailLevel, networkConnectivity.QueryType);
        }

        public static TResult Then<T, TResult>(this Response<T> response, Func<T, TResult> func)
        {
            if (response.Status == ResponseStatus.Ok && response.Data != null)
            {
                return func(response.Data);
            }
            throw new InvalidOperationException("Previous operation failed or returned no data.");
        }
    }

}