using Qoip.ZeroTrustNetwork.Common;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Qoip.ZeroTrustNetwork.NetworkConnectivity.Parsers;
using Qoip.ZeroTrustNetwork.NetworkConnectivity;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class DnsRequest
    {
        public string DomainName { get; }
        public string DnsServer { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }
        public string QueryType { get; }

        public DnsRequest(string domainName, string dnsServer, int timeout, DetailLevel detailLevel, string queryType)
        {
            ArgumentNullException.ThrowIfNull(domainName, nameof(domainName));
            ArgumentNullException.ThrowIfNull(queryType, nameof(queryType));

            DomainName = domainName;
            DnsServer = dnsServer;
            Timeout = timeout;
            DetailLevel = detailLevel;
            QueryType = queryType;
        }

        public Response<DnsResponse> Execute()
        {
            string readableQuery = string.Empty;
            try
            {
                if (!IPAddress.TryParse(DnsServer, out IPAddress? dnsServerAddress))
                {
                    return new Response<DnsResponse>(ResponseStatus.Failure, null, "Invalid DNS server address.");
                }

                var endpoint = new IPEndPoint(dnsServerAddress, 53);
                using (var udpClient = new UdpClient(new IPEndPoint(IPAddress.Any, 0)))
                {
                    udpClient.Client.SendTimeout = Timeout;
                    udpClient.Client.ReceiveTimeout = Timeout;

                    byte[] query = [];
                    (query, readableQuery) = CreateDnsQuery(DomainName, QueryType);
                    udpClient.Send(query, query.Length, endpoint);

                    var serverEndpoint = new IPEndPoint(dnsServerAddress.AddressFamily == AddressFamily.InterNetwork ? IPAddress.Any : IPAddress.IPv6Any, 0);
                    byte[] response = [];
                    response = udpClient.Receive(ref serverEndpoint);

                    // Process the response to extract records
                    var dnsResponseResult = ParseDnsResponse(response);

                    var dnsLookupResponse = new DnsResponse
                    {
                        Server = DnsServer,
                        Address = dnsServerAddress.ToString(),
                        QueryType = QueryType,
                        Records = dnsResponseResult.Records.ToList(),
                        IsAuthoritative = dnsResponseResult.IsAuthoritative,
                        TTL = dnsResponseResult.Ttl,
                        AdditionalDetails = dnsResponseResult.AdditionalDetails
                    };

                    var message = "DNS lookup successful.";
                    if (DetailLevel == DetailLevel.Info || DetailLevel == DetailLevel.Debug)
                    {
                        message += "\n" + BuildResponseInfo(dnsLookupResponse, DomainName, dnsResponseResult.IsAuthoritative);
                    }
                    if (DetailLevel == DetailLevel.Debug)
                    {
                        message += "\nQuery: " + readableQuery;
                    }

                    return new Response<DnsResponse>(ResponseStatus.Ok, dnsLookupResponse, message);
                }
            }
            catch (SocketException ex)
            {
                var errorMessage = $"DNS lookup failed: {ex.Message}";
                if (DetailLevel == DetailLevel.Debug && readableQuery != null)
                {
                    errorMessage += $"\nQuery: {readableQuery}";
                }
                return new Response<DnsResponse>(ResponseStatus.Failure, null, errorMessage);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                if (DetailLevel == DetailLevel.Debug && readableQuery != null)
                {
                    errorMessage += $"\nQuery: {readableQuery}";
                }
                return new Response<DnsResponse>(ResponseStatus.Failure, null, errorMessage);
            }
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

        private DnsResponseResult ParseDnsResponse(byte[] response)
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

            return new DnsResponseResult([.. records], isAuthoritative, ttl, additionalDetails);
        }

        private string BuildResponseInfo(DnsResponse dnsLookupResponse, string domainName, bool isAuthoritative)
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