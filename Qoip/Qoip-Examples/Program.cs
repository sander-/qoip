using Qoip.ZeroTrustNetwork.Common;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;

namespace Qoip_Examples
{
    internal class Program
    {
        static void Main(string[] args)
        {

            // Sample use case for PerformDnsLookup
            var networkConnectivity = new NetworkConnectivity();

            // Perform DNS lookup with default detail level (Ok)
            Console.WriteLine("Performing DNS lookup with default detail level (Ok)...");
            var dnsLookupResponseOk = networkConnectivity.PerformDnsLookup("example.com");
            PrintResponse(dnsLookupResponseOk);

            // Perform DNS lookup with Info detail level
            Console.WriteLine("Performing DNS lookup with Info detail level...");
            var dnsLookupResponseInfo = networkConnectivity.PerformDnsLookup("example.com", detailLevel: DetailLevel.Info);
            PrintResponse(dnsLookupResponseInfo);

            // Perform DNS lookup with Debug detail level
            Console.WriteLine("Performing DNS lookup with Debug detail level...");
            var dnsLookupResponseDebug = networkConnectivity.PerformDnsLookup("example.com", detailLevel: DetailLevel.Debug);
            PrintResponse(dnsLookupResponseDebug);

            // Perform DNS lookup for a CNAME record
            Console.WriteLine("Performing DNS lookup for a CNAME record...");
            var dnsLookupResponseCname = networkConnectivity.PerformDnsLookup("www.example.com", queryType: "CNAME");
            PrintResponse(dnsLookupResponseCname);

            // Perform DNS lookup for a TXT record
            Console.WriteLine("Performing DNS lookup for a TXT record...");
            var dnsLookupResponseTxt = networkConnectivity.PerformDnsLookup("example.com", queryType: "TXT");
            PrintResponse(dnsLookupResponseTxt);

            // Perform DNS lookup for an MX record
            Console.WriteLine("Performing DNS lookup for an MX record...");
            var dnsLookupResponseMx = networkConnectivity.PerformDnsLookup("example.com", queryType: "MX");
            PrintResponse(dnsLookupResponseMx);

            // Perform DNS lookup for a DNSKEY record
            Console.WriteLine("Performing DNS lookup for a DNSKEY record...");
            var dnsLookupResponseDnskey = networkConnectivity.PerformDnsLookup("example.com", queryType: "DNSKEY");
            PrintResponse(dnsLookupResponseDnskey);

            // Perform DNS lookup for an SRV record
            Console.WriteLine("Performing DNS lookup for an SRV record...");
            var dnsLookupResponseSrv = networkConnectivity.PerformDnsLookup("_sip._tcp.example.com", queryType: "SRV");
            PrintResponse(dnsLookupResponseSrv);

            // Perform DNS lookup for an SOA record
            Console.WriteLine("Performing DNS lookup for an SOA record...");
            var dnsLookupResponseSoa = networkConnectivity.PerformDnsLookup("example.com", queryType: "SOA");
            PrintResponse(dnsLookupResponseSoa);

            // Perform DNS lookup for a TLSA record
            Console.WriteLine("Performing DNS lookup for a TLSA record...");
            var dnsLookupResponseTlsa = networkConnectivity.PerformDnsLookup("_443._tcp.example.com", queryType: "TLSA");
            PrintResponse(dnsLookupResponseTlsa);


            //// Sample use case for CheckNetworkAccess
            //var networkAccessResponse = networkConnectivity.CheckNetworkAccess("192.168.1.1");
            //PrintResponse(networkAccessResponse);

            //// Sample use case for CheckFirewallRules
            //var firewallRulesResponse = networkConnectivity.CheckFirewallRules("192.168.1.1");
            //PrintResponse(firewallRulesResponse);

            //// Sample use case for CheckNetworkSegmentation
            //var networkSegmentationResponse = networkConnectivity.CheckNetworkSegmentation("192.168.1.1", "192.168.1.2");
            //PrintResponse(networkSegmentationResponse);

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
