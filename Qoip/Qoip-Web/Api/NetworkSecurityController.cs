using Microsoft.AspNetCore.Mvc;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;

namespace Qoip.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class NetworkSecurityController : ControllerBase
    {
        private readonly INetworkConnectivity _networkConnectivity;

        public NetworkSecurityController(INetworkConnectivity networkConnectivity)
        {
            _networkConnectivity = networkConnectivity;
        }

        [HttpGet("security-headers")]
        public IActionResult PerformSecurityHeaderAnalysis([FromQuery] string url, [FromQuery] int timeout = 5000)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required.");
            }

            var response = _networkConnectivity.ExecuteSecurityHeaderAnalysisRequest(url, timeout);
            return Ok(response);
        }

        [HttpGet("tls-handshake")]
        public IActionResult PerformTlsHandshakeAnalysis([FromQuery] string host, [FromQuery] int port = 443, [FromQuery] int timeout = 5000)
        {
            if (string.IsNullOrEmpty(host))
            {
                return BadRequest("Host is required.");
            }

            var response = _networkConnectivity.ExecuteTlsHandshakeAnalysisRequest(host, port, timeout);
            return Ok(response);
        }

        [HttpGet("email-security")]
        public IActionResult PerformEmailSecurityAnalysis([FromQuery] string domain, [FromQuery] string dkimSelector = "default", [FromQuery] string? dnsServer = null, [FromQuery] int timeout = 5000)
        {
            if (string.IsNullOrEmpty(domain))
            {
                return BadRequest("Domain is required.");
            }

            var response = _networkConnectivity.ExecuteEmailSecurityAnalysisRequest(domain, dkimSelector, dnsServer, timeout);
            return Ok(response);
        }
    }
}
