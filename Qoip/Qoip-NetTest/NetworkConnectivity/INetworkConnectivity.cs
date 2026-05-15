using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using Qoip.ZeroTrustNetwork.NetworkSecurity;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    // Network Connectivity
    public interface INetworkConnectivity
    {
        Response<bool> CheckNetworkAccess(string ipAddress);
        Response<bool> CheckFirewallRules(string ipAddress);
        Response<bool> CheckNetworkSegmentation(string sourceIp, string destinationIp);
        public Response<DnsResponse> ExecuteDnsRequest(string domainName, string? dnsServer = null, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Ok, string queryType = "A");
        public Response<TraceRouteResponse> ExecuteTraceRouteRequest(string ipAddress, int maxHops = 30, int timeout = 2000, bool resolveDns = false, DetailLevel detailLevel = DetailLevel.Info);
        public Response<PortScanResponse> ExecutePortScanRequest(string ipAddress, string portset = "minimal",
            int timeout = 2000, DetailLevel detailLevel = DetailLevel.Info);
        public Response<WhoisResponse> ExecuteWhoisRequest(string ipAddress);
        public Response<HttpProbeResponse> ExecuteHttpProbeRequest(string url, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Info);
        public Response<RangeScanResponse> ExecuteRangeScanRequest(string range, string portSet = "minimal", int timeout = 2000, DetailLevel detailLevel = DetailLevel.Info);
        public Response<DnsSecAnalysisResponse> ExecuteDnsSecAnalysisRequest(string domainName, string? dnsServer = null, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Info);
        public Response<SecurityHeaderAnalysisResponse> ExecuteSecurityHeaderAnalysisRequest(string url, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Info);
        public Response<TlsHandshakeAnalysisResponse> ExecuteTlsHandshakeAnalysisRequest(string host, int port = 443, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Info);
        public Response<EmailSecurityAnalysisResponse> ExecuteEmailSecurityAnalysisRequest(string domainName, string dkimSelector = "default", string? dnsServer = null, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Info);
    }
}


