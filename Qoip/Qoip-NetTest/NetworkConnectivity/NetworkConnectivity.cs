using Qoip.ZeroTrustNetwork.Common;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Text;
using Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class NetworkConnectivity : INetworkConnectivity
    {
        public Response<bool> CheckNetworkAccess(string ipAddress)
        {
            // Implement network access check logic here
            return new Response<bool>(ResponseStatus.Ok, true, "Network access check passed.");
        }

        public Response<bool> CheckFirewallRules(string ipAddress)
        {
            // Implement firewall rules check logic here
            return new Response<bool>(ResponseStatus.Ok, true, "Firewall rules check passed.");
        }

        public Response<bool> CheckNetworkSegmentation(string sourceIp, string destinationIp)
        {
            // Implement network segmentation check logic here
            return new Response<bool>(ResponseStatus.Ok, true, "Network segmentation check passed.");
        }


        public Response<DnsLookupResponse> PerformDnsLookup(string domainName, string dnsServer = null, int timeout = 5000, DetailLevel detailLevel = DetailLevel.Ok, string queryType = "A")
        {
            byte[] query = null;
            string readableQuery = null;
            byte[] response = null;

            try
            {
                if (dnsServer == null)
                {
                    dnsServer = GetSystemDefaultDnsServer();
                }

                if (!IsDnsServerReachable(dnsServer))
                {
                    return new Response<DnsLookupResponse>(ResponseStatus.Failure, null, "DNS server is not reachable.");
                }

                IPAddress dnsServerAddress;
                if (!IPAddress.TryParse(dnsServer, out dnsServerAddress))
                {
                    return new Response<DnsLookupResponse>(ResponseStatus.Failure, null, "Invalid DNS server address.");
                }

                var endpoint = new IPEndPoint(dnsServerAddress, 53);
                using (var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
                {
                    udpClient.Client.SendTimeout = timeout;
                    udpClient.Client.ReceiveTimeout = timeout;

                    (query, readableQuery) = CreateDnsQuery(domainName, queryType);
                    udpClient.Send(query, query.Length, endpoint);

                    var serverEndpoint = new IPEndPoint(dnsServerAddress.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                    response = udpClient.Receive(ref serverEndpoint);

                    // Process the response to extract records
                    var (records, isAuthoritative, ttl, additionalDetails) = ParseDnsResponse(response);

                    var dnsLookupResponse = new DnsLookupResponse
                    {
                        Server = dnsServer,
                        Address = dnsServerAddress.ToString(),
                        QueryType = queryType,
                        Records = records.ToList(),
                        IsAuthoritative = isAuthoritative,
                        TTL = ttl,
                        AdditionalDetails = additionalDetails
                    };

                    var message = "DNS lookup successful.";
                    if (detailLevel == DetailLevel.Info || detailLevel == DetailLevel.Debug)
                    {
                        message += "\n" + BuildResponseInfo(dnsLookupResponse, domainName, isAuthoritative);
                    }
                    if (detailLevel == DetailLevel.Debug)
                    {
                        message += "\nQuery: " + readableQuery;
                    }

                    return new Response<DnsLookupResponse>(ResponseStatus.Ok, dnsLookupResponse, message);
                }
            }
            catch (SocketException ex)
            {
                var errorMessage = $"DNS lookup failed: {ex.Message}";
                if (detailLevel == DetailLevel.Debug && readableQuery != null)
                {
                    errorMessage += $"\nQuery: {readableQuery}";
                }
                return new Response<DnsLookupResponse>(ResponseStatus.Failure, null, errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                if (detailLevel == DetailLevel.Debug && readableQuery != null)
                {
                    errorMessage += $"\nQuery: {readableQuery}";
                }
                return new Response<DnsLookupResponse>(ResponseStatus.Failure, null, errorMessage);
            }
        }


        private bool IsDnsServerReachable(string dnsServer)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = ping.Send(dnsServer);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                return false;
            }
        }

        private string GetSystemDefaultDnsServer()
        {
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                var ipProperties = networkInterface.GetIPProperties();
                var gateways = ipProperties.GatewayAddresses;
                if (gateways.Any(g => g.Address != null && !g.Address.Equals(IPAddress.None)))
                {
                    var dnsAddresses = ipProperties.DnsAddresses;
                    if (dnsAddresses.Count > 0)
                    {
                        return dnsAddresses.First().ToString();
                    }
                }
            }
            throw new InvalidOperationException("No DNS server found in the system's network configuration.");
        }

        private (byte[] query, string readableQuery) CreateDnsQuery(string domainName, string queryType)
        {
            var query = new List<byte>();
            var random = new Random();
            var transactionId = new byte[2];
            random.NextBytes(transactionId);

            // Transaction ID
            query.AddRange(transactionId);

            // Flags: Recursion desired
            query.Add(0x01);
            query.Add(0x00);

            // Questions: 1
            query.Add(0x00);
            query.Add(0x01);

            // Answer RRs: 0
            query.Add(0x00);
            query.Add(0x00);

            // Authority RRs: 0
            query.Add(0x00);
            query.Add(0x00);

            // Additional RRs: 0
            query.Add(0x00);
            query.Add(0x00);

            // Query Name
            var readableQuery = new StringBuilder();
            foreach (var part in domainName.Split('.'))
            {
                query.Add((byte)part.Length);
                query.AddRange(Encoding.ASCII.GetBytes(part));
                readableQuery.Append(part).Append(".");
            }

            // End of Query Name
            query.Add(0x00);
            readableQuery.Append(" ");

            // Query Type
            var queryTypeValue = GetQueryTypeValue(queryType);
            query.Add((byte)(queryTypeValue >> 8));
            query.Add((byte)(queryTypeValue & 0xFF));
            readableQuery.Append(queryType).Append(" ");

            // Query Class: IN (Internet)
            query.Add(0x00);
            query.Add(0x01);
            readableQuery.Append("IN");

            return (query.ToArray(), readableQuery.ToString());
        }

        private int GetQueryTypeValue(string queryType)
        {
            if (QueryTypeValues.Values.TryGetValue(queryType, out var value))
            {
                return value;
            }
            throw new ArgumentException($"Unknown query type: {queryType}");
        }

        private (string[] records, bool isAuthoritative, int ttl, Dictionary<string, string> additionalDetails) ParseDnsResponse(byte[] response)
        {
            var records = new List<string>();
            var additionalDetails = new Dictionary<string, string>();
            var answerCount = (response[6] << 8) | response[7];
            var isAuthoritative = (response[2] & 0x04) != 0;
            var offset = 12;

            // Skip the query section
            while (response[offset] != 0)
            {
                offset += response[offset] + 1;
            }
            offset += 5; // Skip null byte and question type/class

            int ttl = 0;

            for (var i = 0; i < answerCount; i++)
            {
                if (offset + 12 > response.Length) // Ensure there's enough data for the header
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }

                offset += 2; // Skip name pointer
                var type = (response[offset] << 8) | response[offset + 1];
                offset += 2; // Skip type
                offset += 2; // Skip class

                ttl = (response[offset] << 24) | (response[offset + 1] << 16) | (response[offset + 2] << 8) | response[offset + 3];
                offset += 4; // Skip TTL
                var dataLength = (response[offset] << 8) | response[offset + 1];
                offset += 2;

                if (offset + dataLength > response.Length) // Ensure there's enough data for the record
                {
                    throw new IndexOutOfRangeException("Response data is incomplete or corrupted.");
                }

                var parser = DnsResponseParserFactory.GetParser(type);
                records.AddRange(parser.Parse(response, ref offset, dataLength, additionalDetails));
            }

            return (records.ToArray(), isAuthoritative, ttl, additionalDetails);
        }


        private string BuildResponseInfo(DnsLookupResponse dnsLookupResponse, string domainName, bool isAuthoritative)
        {
            var responseInfo = new StringBuilder();
            responseInfo.AppendLine($"Server: {dnsLookupResponse.Server}");
            responseInfo.AppendLine($"Address: {dnsLookupResponse.Address}");
            responseInfo.AppendLine();
            responseInfo.AppendLine(isAuthoritative ? "Authoritative answer:" : "Non-authoritative answer:");
            responseInfo.AppendLine($"Name:    {domainName}");
            responseInfo.AppendLine("Records:  " + string.Join("\n          ", dnsLookupResponse.Records));
            return responseInfo.ToString();
        }
    }
}
