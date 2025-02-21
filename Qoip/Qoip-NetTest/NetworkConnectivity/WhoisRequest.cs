using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Qoip.ZeroTrustNetwork.Common;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class WhoisRequest
    {
        public string TargetAddress { get; }

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

                var whoisServer = "whois.iana.org";
                var whoisData = await QueryWhoisServerAsync(whoisServer, targetIPAddress.ToString());

                if (string.IsNullOrEmpty(whoisData))
                {
                    return new Response<WhoisResponse>(ResponseStatus.Failure, null, "No WHOIS information found.");
                }

                var whoisResponse = new WhoisResponse
                {
                    TargetAddress = TargetAddress,
                    WhoisServer = whoisServer,
                    WhoisData = whoisData
                };

                return new Response<WhoisResponse>(ResponseStatus.Ok, whoisResponse, "WHOIS query successful.");
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                return new Response<WhoisResponse>(ResponseStatus.Failure, null, errorMessage);
            }
        }

        private async Task<string> QueryWhoisServerAsync(string whoisServer, string query)
        {
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
    }
}
