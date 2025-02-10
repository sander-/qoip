using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;
using Qoip.ZeroTrustNetwork.SecurityEncryption;

namespace Qoip_Examples
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //NetworkConnectivityExamples();

            SecurityEncryptionExamples();

        }

        static private void SecurityEncryptionExamples()
        {
            var securityEncryption = new SecurityEncryption();

            // Validate TLS for a given URL
            Console.WriteLine("Validating TLS for a given URL...");
            var tlsValidationResponseA = securityEncryption.ValidateTLS("https://test-ev-rsa.ssl.com/");
            PrintResponse(tlsValidationResponseA);

            // Validate TLS for a given URL
            Console.WriteLine("Validating TLS for a given URL known to be expired...");
            var tlsValidationResponseB = securityEncryption.ValidateTLS("https://expired-rsa-dv.ssl.com/");
            PrintResponse(tlsValidationResponseB);

            // Validate TLS for a revoked certificate
            Console.WriteLine("Validating TLS for a given URL known to have a revoked certificate...");
            var tlsValidationResponseC = securityEncryption.ValidateTLS("https://revoked-rsa-dv.ssl.com/");
            PrintResponse(tlsValidationResponseC);

        }

        static private void NetworkConnectivityExamples()
        {

            // Sample use case for ExecuteDnsRequest
            var networkConnectivity = new NetworkConnectivity();

            // Perform DNS lookup with default detail level (Ok)
            Console.WriteLine("Performing DNS lookup with default detail level (Ok)...");
            var dnsLookupResponseOk = networkConnectivity.ExecuteDnsRequest("example.com");
            PrintResponse(dnsLookupResponseOk);

            // Perform DNS lookup with Info detail level
            Console.WriteLine("Performing DNS lookup with Info detail level...");
            var dnsLookupResponseInfo = networkConnectivity.ExecuteDnsRequest("example.com", detailLevel: DetailLevel.Info);
            PrintResponse(dnsLookupResponseInfo);

            // Perform DNS lookup with Debug detail level
            Console.WriteLine("Performing DNS lookup with Debug detail level...");
            var dnsLookupResponseDebug = networkConnectivity.ExecuteDnsRequest("example.com", detailLevel: DetailLevel.Debug);
            PrintResponse(dnsLookupResponseDebug);

            // Perform DNS lookup for a CNAME record
            Console.WriteLine("Performing DNS lookup for a CNAME record...");
            var dnsLookupResponseCname = networkConnectivity.ExecuteDnsRequest("www.example.com", queryType: "CNAME");
            PrintResponse(dnsLookupResponseCname);

            // Perform DNS lookup for an NS record
            Console.WriteLine("Performing DNS lookup for an NS record...");
            var dnsLookupResponseNs = networkConnectivity.ExecuteDnsRequest("example.com", queryType: "NS");
            PrintResponse(dnsLookupResponseNs);

            // Perform DNS lookup for an A record from the authoritative DNS server
            // We use the result from the previous NS lookup to specify the DNS server
            Console.WriteLine("Performing DNS lookup for an A record with a authoritative DNS server...");
            var dnsLookupResponseA = networkConnectivity
                .WithQueryType("NS")
                .ExecuteDnsRequest("example.com")
                .Then(result => networkConnectivity
                    .WithQueryType("A")
                    .ExecuteDnsRequest(result.FirstRecord)
                .Then(result => networkConnectivity
                    .WithDnsServer(result.FirstRecord)
                    .WithQueryType("A")
                    .ExecuteDnsRequest("example.com")));

            PrintResponse(dnsLookupResponseA);

            // Perform DNS lookup for a TXT record
            Console.WriteLine("Performing DNS lookup for a TXT record...");
            var dnsLookupResponseTxt = networkConnectivity.ExecuteDnsRequest("example.com", queryType: "TXT");
            PrintResponse(dnsLookupResponseTxt);

            // Perform DNS lookup for an MX record
            Console.WriteLine("Performing DNS lookup for an MX record...");
            var dnsLookupResponseMx = networkConnectivity.ExecuteDnsRequest("qoip.com", queryType: "MX");
            PrintResponse(dnsLookupResponseMx);

            // Perform DNS lookup for a DNSKEY record
            Console.WriteLine("Performing DNS lookup for a DNSKEY record...");
            var dnsLookupResponseDnskey = networkConnectivity.ExecuteDnsRequest("example.com", queryType: "DNSKEY");
            PrintResponse(dnsLookupResponseDnskey);

            // Perform DNS lookup for an SOA record
            Console.WriteLine("Performing DNS lookup for an SOA record...");
            var dnsLookupResponseSoa = networkConnectivity.ExecuteDnsRequest("example.com", queryType: "SOA", detailLevel: DetailLevel.Debug);
            PrintResponse(dnsLookupResponseSoa);

            // Perform DNS lookup for a TLSA record
            Console.WriteLine("Performing DNS lookup for a TLSA record...");
            var dnsLookupResponseTlsa = networkConnectivity.ExecuteDnsRequest("_443._tcp.good-pkixta.dane.huque.com", queryType: "TLSA");
            PrintResponse(dnsLookupResponseTlsa);
        }

        static void PrintResponse<T>(Response<T> response)
        {
            Console.WriteLine($"Status: {response.Status}");
            Console.WriteLine($"Message: {response.Message}");
            if (response.Data != null)
            {
                Console.WriteLine($"Data: {response.Data}");
            }
            Console.WriteLine();
        }
    }
}
