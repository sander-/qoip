using Qoip.ZeroTrustNetwork.Common;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class NetworkConnectivity : INetworkConnectivity
    {
        public string DnsServer { get; set; }
        public string QueryType { get; set; } = "A";
        public int Timeout { get; set; } = 5000;
        public int MaxHops { get; set; } = 30;
        public bool ResolveDns { get; set; } = false;
        public DetailLevel DetailLevel { get; set; } = DetailLevel.Ok;


        public Response<bool> CheckNetworkAccess(string ipAddress)
        {
            // Implement network access check logic here
            return new Response<bool>(ResponseStatus.Ok, true, "Network access check passed.");
        }

        public Response<bool> CheckFirewallRules(string ipAddress)
        {
            // Implement firewall rules check logic here
            return new Response<bool>(ResponseStatus.Ok, true, "Firewall rules check passed.");
        }

        public Response<bool> CheckNetworkSegmentation(string sourceIp, string destinationIp)
        {
            // Implement network segmentation check logic here
            return new Response<bool>(ResponseStatus.Ok, true, "Network segmentation check passed.");
        }

        public Response<TraceRouteResponse> ExecuteTraceRouteRequest(string ipAddress, int maxHops = 30, int timeout = 2000, bool resolveDns = false, DetailLevel detailLevel = DetailLevel.Info)
        {
            var traceRouteRequest = new TraceRouteRequest(ipAddress, maxHops, timeout, resolveDns, detailLevel);
            return traceRouteRequest.Execute();
        }

        public Response<TraceRouteResponse> ExecuteTraceRouteRequest(string ipAddress)
        {
            return ExecuteTraceRouteRequest(ipAddress, MaxHops, Timeout, ResolveDns, DetailLevel);
        }

        public Response<DnsResponse> ExecuteDnsRequest(string domainName, string? dnsServer = null, int timeout = 2000, DetailLevel detailLevel = DetailLevel.Ok, string queryType = "A")
        {
            dnsServer ??= GetSystemDefaultDnsServer();

            if (!IsDnsServerReachable(dnsServer))
            {
                return new Response<DnsResponse>(ResponseStatus.Failure, null, "DNS server is not reachable.");
            }

            var dnsRequest = new DnsRequest(domainName, dnsServer, timeout, detailLevel, queryType);
            return dnsRequest.Execute();
        }

        public Response<DnsResponse> ExecuteDnsRequest(string domainName)
        {
            return ExecuteDnsRequest(domainName, DnsServer, Timeout, DetailLevel, QueryType);
        }

        private bool IsDnsServerReachable(string dnsServer)
        {
            try
            {
                using var ping = new Ping();
                var reply = ping.Send(dnsServer);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private string GetSystemDefaultDnsServer()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var gateways = ipProperties.GatewayAddresses;
                if (gateways.Any(g => g.Address != null && !g.Address.Equals(IPAddress.None)))
                {
                    var dnsAddresses = ipProperties.DnsAddresses;
                    if (dnsAddresses.Count > 0)
                    {
                        return dnsAddresses.First().ToString();
                    }
                }
            }
            throw new InvalidOperationException("No DNS server found in the system's network configuration.");
        }

    }
}
