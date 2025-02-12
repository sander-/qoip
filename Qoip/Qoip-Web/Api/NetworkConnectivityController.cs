using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using Qoip.ZeroTrustNetwork.SecurityEncryption;

namespace Qoip.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkConnectivityController : ControllerBase
    {
        private readonly INetworkConnectivity _networkConnectivity;

        public NetworkConnectivityController(INetworkConnectivity networkConnectivity)
        {
            _networkConnectivity = networkConnectivity;

        }

        [HttpGet("dns")]
        public IActionResult PerformDnsRequest(string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return BadRequest("Domain is required.");
            }

            var dnsResponse = _networkConnectivity.ExecuteDnsRequest(domain);
            return Ok(dnsResponse);
        }

        [HttpGet("traceroute")]
        public IActionResult PerformTraceRoute(string host, int maxHops = 30, int timeout = 2000, bool resolveDns = false)
        {
            // Check the host for IP address or domain name
            // If the host is an IP address, use it directly
            // If the host is a domain name, resolve it to an IP address
            var ipAddress = host;
            if (!System.Net.IPAddress.TryParse(host, out var ip))
            {
                // If the host is not an IP address, resolve it to an IP address
                var dnsResponse = _networkConnectivity.ExecuteDnsRequest(host);
                if (dnsResponse.Status != Qoip.ZeroTrustNetwork.Common.ResponseStatus.Ok)
                {
                    return BadRequest("Failed to resolve the host to an IP address.");
                }
                ipAddress = dnsResponse.Data.FirstRecord;
            }

            var traceRouteResponse = _networkConnectivity.ExecuteTraceRouteRequest(ipAddress, maxHops, timeout, resolveDns);
            return Ok(traceRouteResponse);
        }

    }
}
