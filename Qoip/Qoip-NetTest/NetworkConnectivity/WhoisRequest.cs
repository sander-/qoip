using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Qoip.ZeroTrustNetwork.Common;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class WhoisRequest
    {
        public string TargetAddress { get; }

        private static readonly string[] InitialWhoisServers = new[]
        {
            "whois.iana.org",
            "whois.arin.net",
            "whois.ripe.net",
            "whois.apnic.net",
            "whois.lacnic.net",
            "whois.afrinic.net"
        };

        public WhoisRequest(string targetAddress)
        {
            ArgumentNullException.ThrowIfNull(targetAddress, nameof(targetAddress));
            TargetAddress = targetAddress;
        }

        public Response<WhoisResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<WhoisResponse>> ExecuteAsync()
        {
            try
            {
                if (!IPAddress.TryParse(TargetAddress, out IPAddress? targetIPAddress))
                {
                    return new Response<WhoisResponse>(ResponseStatus.Failure, null, "Invalid target address.");
                }

                foreach (var whoisServer in InitialWhoisServers)
                {
                    var whoisData = await QueryWhoisServerAsync(whoisServer, targetIPAddress.ToString());

                    if (!string.IsNullOrEmpty(whoisData))
                    {
                        var referServer = ExtractReferServer(whoisData);
                        if (!string.IsNullOrEmpty(referServer))
                        {
                            whoisData = await QueryWhoisServerAsync(referServer, targetIPAddress.ToString());
                        }

                        var whoisResponse = new WhoisResponse
                        {
                            TargetAddress = TargetAddress,
                            WhoisServer = referServer,
                            WhoisData = whoisData
                        };

                        return new Response<WhoisResponse>(ResponseStatus.Ok, whoisResponse, "WHOIS query successful.");
                    }
                }

                return new Response<WhoisResponse>(ResponseStatus.Failure, null, "No WHOIS information found.");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                return new Response<WhoisResponse>(ResponseStatus.Failure, null, errorMessage);
            }
        }

        private async Task<string> QueryWhoisServerAsync(string whoisServer, string query)
        {
            // include n in query to get more detailed information
            query = $"{query}";

            using (var tcpClient = new TcpClient())
            {
                await tcpClient.ConnectAsync(whoisServer, 43);
                using (var networkStream = tcpClient.GetStream())
                {
                    var queryBytes = Encoding.ASCII.GetBytes(query + "\r\n");
                    await networkStream.WriteAsync(queryBytes, 0, queryBytes.Length);

                    using (var reader = new StreamReader(networkStream, Encoding.ASCII))
                    {
                        var response = await reader.ReadToEndAsync();
                        return response;
                    }
                }
            }
        }

        private string ExtractReferServer(string whoisData)
        {
            var match = Regex.Match(whoisData, @"refer:\s*(\S+)", RegexOptions.IgnoreCase);
            return match.Success ? match.Groups[1].Value : null;
        }
    }
}
