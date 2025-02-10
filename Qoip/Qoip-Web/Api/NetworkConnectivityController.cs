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
        public IActionResult PerformDnsRequest([FromQuery] string domain)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return BadRequest("Domain is required.");
            }

            var dnsResponse = _networkConnectivity.ExecuteDnsRequest(domain);
            return Ok(dnsResponse);
        }
    }
}
