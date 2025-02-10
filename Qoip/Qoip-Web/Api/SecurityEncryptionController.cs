using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using Qoip.ZeroTrustNetwork.SecurityEncryption;

namespace Qoip.Web.Api
{
    [Route("api/[controller]")]
    [ApiController]
    public class SecurityEncryptionController : ControllerBase
    {   
        private readonly ISecurityEncryption _securityEncryption;
        public SecurityEncryptionController( ISecurityEncryption securityEncryption)
        {   
            _securityEncryption = securityEncryption;
        }

        [HttpGet("certificate")]
        public IActionResult PerformCertificateValidation([FromQuery] string url, [FromQuery] int expirationWarningThresholdInDays = 0)
        {
            if (string.IsNullOrEmpty(url))
            {
                return BadRequest("URL is required.");
            }

            var certificateValidationRequest = new CertificateValidationRequest(url, expirationWarningThresholdInDays);
            var response = certificateValidationRequest.Execute();
            return Ok(response);
        }
    }
}
