using Qoip.ZeroTrustNetwork.Common;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class TraceRouteRequest
    {
        public string TargetAddress { get; }
        public int MaxHops { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }
        public bool ResolveDns { get; }

        public TraceRouteRequest(string targetAddress, int maxHops, int timeout, bool resolveDns, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(targetAddress, nameof(targetAddress));

            TargetAddress = targetAddress;
            MaxHops = maxHops;
            Timeout = timeout;
            DetailLevel = detailLevel;
            ResolveDns = resolveDns;
        }

        public Response<TraceRouteResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<TraceRouteResponse>> ExecuteAsync()
        {
            var traceResults = new TraceRouteResponse
            {
                TargetAddress = TargetAddress,
                MaxHops = MaxHops,
                Timeout = Timeout,
                DetailLevel = DetailLevel,
                TraceResults = new List<TraceRouteResult>()
            };

            var stopwatch = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                if (!IPAddress.TryParse(TargetAddress, out IPAddress? targetIPAddress))
                {
                    return new Response<TraceRouteResponse>(ResponseStatus.Failure, null, "Invalid target address.");
                }

                var tasks = new List<Task<TraceRouteResult>>();

                for (int ttl = 1; ttl <= MaxHops; ttl++)
                {
                    tasks.Add(SendPingAsync(targetIPAddress, ttl));
                }

                var results = await Task.WhenAll(tasks);

                var fallbackTasks = new List<Task>();

                foreach (var result in results)
                {
                    if (result.Status == "No response" || result.Status == "TimedOut")
                    {
                        fallbackTasks.Add(Task.Run(async () =>
                        {
                            // Try using UDP to infer the IP address of the missing hop
                            var udpResult = await SendUdpProbeAsync(targetIPAddress, result.Hop);
                            if (udpResult != null)
                            {
                                result.IpAddress = udpResult.Address?.ToString() ?? "Unknown";
                                result.Hostname = ResolveDns ? GetHostName(udpResult.Address) : "Unknown";
                                result.Status = "Inferred via UDP";
                            }
                            else
                            {
                                // Try using TCP to infer the IP address of the missing hop
                                var tcpResult = await SendTcpProbeAsync(targetIPAddress, result.Hop);
                                if (tcpResult != null && tcpResult.Address != null && !tcpResult.Address.Equals(targetIPAddress))
                                {
                                    result.IpAddress = tcpResult.Address.ToString();
                                    result.Hostname = ResolveDns ? GetHostName(tcpResult.Address) : "Unknown";
                                    result.Status = "Inferred via TCP";
                                }
                                else
                                {
                                    result.Status = "No response";
                                }
                            }
                        }));
                    }

                    traceResults.TraceResults.Add(result);

                    if (result.Status == "Success")
                    {
                        stopwatch.Stop();
                        traceResults.TotalResponseTime = (int)stopwatch.ElapsedMilliseconds;
                        traceResults.TotalHops = result.Hop;
                        return new Response<TraceRouteResponse>(ResponseStatus.Ok, traceResults, $"Traceroute completed in {traceResults.TotalResponseTime} ms.");
                    }
                }

                await Task.WhenAll(fallbackTasks);

                stopwatch.Stop();
                traceResults.TotalResponseTime = (int)stopwatch.ElapsedMilliseconds;
                traceResults.TotalHops = traceResults.TraceResults.Count;

                var message = $"Traceroute completed in {traceResults.TotalResponseTime} ms.";
                if (DetailLevel == DetailLevel.Info || DetailLevel == DetailLevel.Debug)
                {
                    message += "\n" + string.Join("\n", traceResults.TraceResults.Select(r => $"{r.Hop}: {r.IpAddress} - {r.RoundTripTime} ms ({r.Hostname})"));
                }

                return new Response<TraceRouteResponse>(ResponseStatus.Ok, traceResults, message);
            }
            catch (Exception ex)
            {
                var errorMessage = $"An error occurred: {ex.Message}";
                return new Response<TraceRouteResponse>(ResponseStatus.Failure, null, errorMessage);
            }
        }

        private async Task<TraceRouteResult> SendPingAsync(IPAddress targetIPAddress, int ttl)
        {
            using (var ping = new Ping())
            {
                var pingOptions = new PingOptions(ttl, true);
                var buffer = Encoding.ASCII.GetBytes("TraceRouteTest");

                try
                {
                    var reply = await ping.SendPingAsync(targetIPAddress, Timeout, buffer, pingOptions);
                    return new TraceRouteResult
                    {
                        Hop = ttl,
                        IpAddress = reply.Status == IPStatus.TimedOut ? "Unknown" : reply.Address?.ToString() ?? "Unknown",
                        Hostname = reply.Status == IPStatus.TimedOut ? "Unknown" : ResolveDns && reply.Address != null ? GetHostName(reply.Address) : "Unknown",
                        RoundTripTime = reply.RoundtripTime,
                        Status = reply.Status == IPStatus.TtlExpired ? "HopReached" : reply.Status.ToString()
                    };
                }
                catch
                {
                    return new TraceRouteResult
                    {
                        Hop = ttl,
                        IpAddress = "Unknown",
                        Hostname = "Unknown",
                        RoundTripTime = -1,
                        Status = "No response"
                    };
                }
            }
        }

        private async Task<IPEndPoint?> SendUdpProbeAsync(IPAddress targetIPAddress, int ttl)
        {
            try
            {
                using (var udpClient = new UdpClient())
                {
                    udpClient.Client.ReceiveTimeout = Timeout;
                    udpClient.Client.Ttl = (short)ttl;

                    var endpoint = new IPEndPoint(targetIPAddress, 33434);
                    var buffer = Encoding.ASCII.GetBytes("TraceRouteTest");
                    await udpClient.SendAsync(buffer, buffer.Length, endpoint);

                    var responseEndpoint = new IPEndPoint(IPAddress.Any, 0);
                    var receiveTask = udpClient.ReceiveAsync();
                    if (await Task.WhenAny(receiveTask, Task.Delay(Timeout)) == receiveTask)
                    {
                        var responseBuffer = receiveTask.Result.Buffer;
                        responseEndpoint = receiveTask.Result.RemoteEndPoint;
                        return responseEndpoint;
                    }
                    else
                    {
                        return null; // Receive timed out
                    }
                }
            }
            catch (SocketException)
            {
                return null;
            }
        }

        private async Task<IPEndPoint?> SendTcpProbeAsync(IPAddress targetIPAddress, int ttl)
        {
            try
            {
                using (var tcpClient = new TcpClient())
                {
                    tcpClient.ReceiveTimeout = Timeout; // Set the receive timeout
                    tcpClient.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.IpTimeToLive, ttl);

                    var endpoint = new IPEndPoint(targetIPAddress, 33434);
                    var connectTask = tcpClient.ConnectAsync(endpoint.Address, endpoint.Port);
                    if (await Task.WhenAny(connectTask, Task.Delay(Timeout)) == connectTask)
                    {
                        // Connection successful
                        return (IPEndPoint)tcpClient.Client.RemoteEndPoint;
                    }
                    else
                    {
                        // Connection timed out
                        return null;
                    }
                }
            }
            catch (SocketException ex)
            {
                if (ex.SocketErrorCode == SocketError.TimedOut || ex.SocketErrorCode == SocketError.HostUnreachable)
                {
                    return null;
                }
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in SendTcpProbeAsync: {ex.Message}");
                return null;
            }
        }

        private IPEndPoint? SendUdpProbe(IPAddress targetIPAddress, int ttl)
        {
            return SendUdpProbeAsync(targetIPAddress, ttl).GetAwaiter().GetResult();
        }

        private IPEndPoint? SendTcpProbe(IPAddress targetIPAddress, int ttl)
        {
            return SendTcpProbeAsync(targetIPAddress, ttl).GetAwaiter().GetResult();
        }

        private string GetHostName(IPAddress address)
        {
            try
            {
                var hostEntry = Dns.GetHostEntry(address);
                return hostEntry.HostName;
            }
            catch (SocketException)
            {
                return "DNS resolution failed";
            }
        }
    }
}

