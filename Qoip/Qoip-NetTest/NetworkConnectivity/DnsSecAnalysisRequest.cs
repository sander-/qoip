using Qoip.ZeroTrustNetwork.Common;
using System.Net;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class DnsSecAnalysisRequest
    {
        public string DomainName { get; }
        public string DnsServer { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }

        public DnsSecAnalysisRequest(string domainName, string dnsServer, int timeout, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(domainName);
            ArgumentNullException.ThrowIfNull(dnsServer);

            DomainName = domainName;
            DnsServer = dnsServer;
            Timeout = timeout;
            DetailLevel = detailLevel;
        }

        public Response<DnsSecAnalysisResponse> Execute()
        {
            try
            {
                var dnsKeyResponse = new DnsRequest(DomainName, DnsServer, Timeout, DetailLevel, "DNSKEY").Execute();
                var rrSigResponse = new DnsRequest(DomainName, DnsServer, Timeout, DetailLevel, "RRSIG").Execute();
                var nsecResponse = new DnsRequest(DomainName, DnsServer, Timeout, DetailLevel, "NSEC").Execute();
                var nsec3Response = new DnsRequest(DomainName, DnsServer, Timeout, DetailLevel, "NSEC3").Execute();
                var dsResponse = new DnsRequest(DomainName, DnsServer, Timeout, DetailLevel, "DS").Execute();

                var response = new DnsSecAnalysisResponse
                {
                    DomainName = DomainName,
                    DnsServer = DnsServer,
                    DetailLevel = DetailLevel,
                    DnsKeyRecords = dnsKeyResponse.Data?.Records ?? new List<string>(),
                    RrSigRecords = rrSigResponse.Data?.Records ?? new List<string>(),
                    NsecRecords = nsecResponse.Data?.Records ?? new List<string>(),
                    Nsec3Records = nsec3Response.Data?.Records ?? new List<string>(),
                    DsRecords = dsResponse.Data?.Records ?? new List<string>()
                };

                response.HasDnsKey = response.DnsKeyRecords.Count > 0;
                response.HasRrSig = response.RrSigRecords.Count > 0;
                response.HasNsec = response.NsecRecords.Count > 0;
                response.HasNsec3 = response.Nsec3Records.Count > 0;
                response.HasDs = response.DsRecords.Count > 0;

                if (!response.HasDnsKey)
                {
                    response.Findings.Add("No DNSKEY records were found.");
                }

                if (!response.HasRrSig)
                {
                    response.Findings.Add("No RRSIG records were found.");
                }

                if (!response.HasNsec && !response.HasNsec3)
                {
                    response.Findings.Add("No NSEC or NSEC3 records were found.");
                }

                if (!response.HasDs)
                {
                    response.Findings.Add("No DS records were found.");
                }

                if (response.HasDnsKey && response.HasRrSig)
                {
                    response.Findings.Add("DNSSEC-related records were detected for the domain.");
                }

                var status = response.HasDnsKey && response.HasRrSig ? ResponseStatus.Ok : ResponseStatus.Warning;
                return new Response<DnsSecAnalysisResponse>(status, response, "DNSSEC analysis completed.");
            }
            catch (Exception ex)
            {
                return new Response<DnsSecAnalysisResponse>(ResponseStatus.Failure, null, $"DNSSEC analysis failed: {ex.Message}");
            }
        }
    }
}
