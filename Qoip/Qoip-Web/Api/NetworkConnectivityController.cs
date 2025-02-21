using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using Qoip.ZeroTrustNetwork.SecurityEncryption;
using System.Net.Sockets;
using System.Net;

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
            // in the response, obfuscate the first result  
            if (traceRouteResponse.Status == Qoip.ZeroTrustNetwork.Common.ResponseStatus.Ok)
            {
                traceRouteResponse.Data.TraceResults[0].IpAddress = "xxx.xxx.xxx.xxx";
                traceRouteResponse.Data.TraceResults[0].Hostname = "***";
            }
            return Ok(traceRouteResponse);
        }

        [HttpGet("client-ip")]
        public IActionResult GetClientIpAddress()
        {
            var clientIpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            var xForwardedForHeader = HttpContext.Request.Headers["X-Forwarded-For"].ToString();
            var proxyAddresses = xForwardedForHeader.Split(',').Select(ip => ip.Trim()).ToList();
            var realIpAddress = HttpContext.Request.Headers["X-Real-IP"].ToString();

            var validProxyAddresses = proxyAddresses.Where(ip => System.Net.IPAddress.TryParse(ip, out _)).ToList();
            var ipv4Address = validProxyAddresses.FirstOrDefault(ip => System.Net.IPAddress.TryParse(ip, out var parsedIp) && parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
            var ipv6Address = validProxyAddresses.FirstOrDefault(ip => System.Net.IPAddress.TryParse(ip, out var parsedIp) && parsedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6);

            if (ipv4Address == clientIpAddress)
            {
                validProxyAddresses.Clear();
            }

            var clientIpCanonical = GetCanonicalName(clientIpAddress);
            var ipv4Canonical = GetCanonicalName(ipv4Address);
            var ipv6Canonical = GetCanonicalName(ipv6Address);

            var result = new
            {
                ClientIpAddress = clientIpAddress,
                ClientIpCanonical = clientIpCanonical,
                IPv4Address = ipv4Address,
                IPv4Canonical = ipv4Canonical,
                IPv6Address = ipv6Address,
                IPv6Canonical = ipv6Canonical,
                ProxyAddresses = validProxyAddresses,
                RealIpAddress = realIpAddress
            };

            return Ok(result);
        }

        [HttpGet("whois")]
        public async Task<IActionResult> GetWhoisInfo(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return BadRequest("IP address is required.");
            }

            var whoisRequest = new WhoisRequest(ipAddress);
            var whoisResponse = await whoisRequest.ExecuteAsync();
            return Ok(whoisResponse.Data);
        }

        private string GetCanonicalName(string ipAddress)
        {
            if (string.IsNullOrEmpty(ipAddress))
            {
                return null;
            }

            try
            {
                var hostEntry = Dns.GetHostEntry(ipAddress);
                return hostEntry.HostName;
            }
            catch (SocketException)
            {
                return null;
            }
        }
    }
}
