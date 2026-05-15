using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;

namespace Qoip.ZeroTrustNetwork.NetworkSecurity
{
    public class EmailSecurityAnalysisRequest
    {
        public string DomainName { get; }
        public string? DnsServer { get; }
        public string DkimSelector { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }

        public EmailSecurityAnalysisRequest(string domainName, string? dnsServer, string dkimSelector, int timeout, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(domainName);
            ArgumentNullException.ThrowIfNull(dkimSelector);

            DomainName = domainName;
            DnsServer = dnsServer;
            DkimSelector = dkimSelector;
            Timeout = timeout;
            DetailLevel = detailLevel;
        }

        public Response<EmailSecurityAnalysisResponse> Execute()
        {
            try
            {
                var mxResponse = new DnsRequest(DomainName, ResolveDnsServer(), Timeout, DetailLevel, "MX").Execute();
                var txtResponse = new DnsRequest(DomainName, ResolveDnsServer(), Timeout, DetailLevel, "TXT").Execute();
                var dmarcResponse = new DnsRequest($"_dmarc.{DomainName}", ResolveDnsServer(), Timeout, DetailLevel, "TXT").Execute();
                var dkimResponse = new DnsRequest($"{DkimSelector}._domainkey.{DomainName}", ResolveDnsServer(), Timeout, DetailLevel, "TXT").Execute();

                var response = new EmailSecurityAnalysisResponse
                {
                    DomainName = DomainName,
                    DetailLevel = DetailLevel,
                    MxRecords = mxResponse.Data?.Records ?? new List<string>(),
                    SpfRecords = txtResponse.Data?.Records?.Where(record => record.StartsWith("v=spf1", StringComparison.OrdinalIgnoreCase)).ToList() ?? new List<string>(),
                    DmarcRecord = dmarcResponse.Data?.Records?.FirstOrDefault(record => record.StartsWith("v=DMARC1", StringComparison.OrdinalIgnoreCase)) ?? string.Empty,
                    DkimRecord = dkimResponse.Data?.Records?.FirstOrDefault() ?? string.Empty
                };

                response.HasMxRecords = response.MxRecords.Count > 0;
                response.HasSpfRecord = response.SpfRecords.Count > 0;
                response.HasDmarcRecord = !string.IsNullOrWhiteSpace(response.DmarcRecord);
                response.HasDkimRecord = !string.IsNullOrWhiteSpace(response.DkimRecord);

                if (!response.HasMxRecords)
                {
                    response.Findings.Add("No MX records were found for the domain.");
                }

                if (!response.HasSpfRecord)
                {
                    response.Findings.Add("No SPF record was found for the domain.");
                }

                if (!response.HasDmarcRecord)
                {
                    response.Findings.Add("No DMARC record was found for the domain.");
                }

                if (!response.HasDkimRecord)
                {
                    response.Findings.Add($"No DKIM record was found for selector '{DkimSelector}'.");
                }

                var status = response.Findings.Count == 0 ? ResponseStatus.Ok : ResponseStatus.Warning;
                return new Response<EmailSecurityAnalysisResponse>(status, response, "Email security analysis completed.");
            }
            catch (Exception ex)
            {
                return new Response<EmailSecurityAnalysisResponse>(ResponseStatus.Failure, null, $"Email security analysis failed: {ex.Message}");
            }
        }

        private string ResolveDnsServer()
        {
            return string.IsNullOrWhiteSpace(DnsServer)
                ? "8.8.8.8"
                : DnsServer;
        }
    }
}
