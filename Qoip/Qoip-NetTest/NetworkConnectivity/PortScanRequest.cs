using Qoip.ZeroTrustNetwork.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class PortScanRequest
    {
        private static readonly int[] MinimalPorts = { 80, 443 };
        private static readonly int[] CommonPorts = { 21, 22, 23, 25, 53, 80, 110, 135, 139, 143, 443, 445, 587, 3389, 8080 };
        private static readonly int[] AllPorts = Enumerable.Range(1, 65535).ToArray();

        public DetailLevel DetailLevel { get; set; }
        public IEnumerable<int> PortsToScan { get; set; }
        public string IPAddress { get; set; }
        public int Timeout { get; set; }

        public PortScanRequest(string ipAddress, string portSet, int timeout = 1000, DetailLevel detailLevel = DetailLevel.Ok)
        {
            DetailLevel = detailLevel;
            Timeout = timeout;
            PortsToScan = portSet switch
            {
                "minimal" => MinimalPorts,
                "common" => CommonPorts,
                "all" => AllPorts,
                _ => throw new ArgumentException("Invalid port set specified")
            };
            IPAddress = ipAddress;
        }

        public Response<PortScanResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<PortScanResponse>> ExecuteAsync()
        {
            var scanResults = new ConcurrentDictionary<int, bool>();
            var scanStartTime = DateTime.UtcNow;

            var tasks = PortsToScan.Select(port => Task.Run(async () =>
            {
                bool isOpen = await IsPortOpenAsync(IPAddress, port);
                scanResults[port] = isOpen;
                return isOpen;
            }));

            await Task.WhenAll(tasks);

            var scanEndTime = DateTime.UtcNow;

            return new Response<PortScanResponse>(ResponseStatus.Ok,
                CreatePortScanResponse(scanResults, scanStartTime, scanEndTime), "Port scan completed successfully");
        }

        private PortScanResponse CreatePortScanResponse(ConcurrentDictionary<int, bool> scanResults, DateTime scanStartTime, DateTime scanEndTime)
        {
            return new PortScanResponse
            {
                IpAddress = IPAddress,
                ScanStartTime = scanStartTime,
                ScanEndTime = scanEndTime,
                DetailLevel = DetailLevel,
                UnorderedPortResults = scanResults.OrderBy(kv => kv.Key).ToDictionary(kv => kv.Key, kv => kv.Value)
            };
        }

        private async Task<bool> IsPortOpenAsync(string ipAddress, int port)
        {
            try
            {
                using var client = new TcpClient();
                var connectTask = client.ConnectAsync(ipAddress, port);
                var timeoutTask = Task.Delay(Timeout);

                var completedTask = await Task.WhenAny(connectTask, timeoutTask);
                return completedTask == connectTask && client.Connected;
            }
            catch
            {
                return false;
            }
        }
    }
}
