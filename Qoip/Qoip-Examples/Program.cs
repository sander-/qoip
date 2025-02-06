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
            var dnsLookupResponseOk = networkConnectivity.PerformDnsLookup("example.com");
            PrintResponse(dnsLookupResponseOk);

            // Perform DNS lookup with Info detail level
            var dnsLookupResponseInfo = networkConnectivity.PerformDnsLookup("example.com", detailLevel: DetailLevel.Info);
            PrintResponse(dnsLookupResponseInfo);

            // Perform DNS lookup with Debug detail level
            var dnsLookupResponseDebug = networkConnectivity.PerformDnsLookup("example.com", detailLevel: DetailLevel.Debug);
            PrintResponse(dnsLookupResponseDebug);

            // Perform DNS lookup for a CNAME record
            var dnsLookupResponseCname = networkConnectivity.PerformDnsLookup("www.example.com", queryType: "CNAME");
            PrintResponse(dnsLookupResponseCname);

            // Perform DNS lookup for a TXT record
            var dnsLookupResponseTxt = networkConnectivity.PerformDnsLookup("example.com", queryType: "TXT");
            PrintResponse(dnsLookupResponseTxt);


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
