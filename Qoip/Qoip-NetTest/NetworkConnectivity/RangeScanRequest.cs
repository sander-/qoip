using Qoip.ZeroTrustNetwork.Common;
using System.Net;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class RangeScanRequest
    {
        public string Range { get; }
        public string PortSet { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }

        public RangeScanRequest(string range, string portSet, int timeout, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(range);
            ArgumentNullException.ThrowIfNull(portSet);

            Range = range;
            PortSet = portSet;
            Timeout = timeout;
            DetailLevel = detailLevel;
        }

        public Response<RangeScanResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<RangeScanResponse>> ExecuteAsync()
        {
            try
            {
                var hosts = ExpandRange(Range).ToList();
                var response = new RangeScanResponse
                {
                    InputRange = Range,
                    PortSet = PortSet,
                    DetailLevel = DetailLevel,
                    ScanStartTime = DateTime.UtcNow,
                    ScannedHosts = hosts
                };

                foreach (var host in hosts)
                {
                    var portScanResponse = await new PortScanRequest(host, PortSet, Timeout, DetailLevel).ExecuteAsync();
                    if (portScanResponse.Data != null && portScanResponse.Data.OpenPorts.Count > 0)
                    {
                        response.HostsWithOpenPorts[host] = portScanResponse.Data.OpenPorts;
                    }
                }

                response.ScanEndTime = DateTime.UtcNow;
                return new Response<RangeScanResponse>(ResponseStatus.Ok, response, $"Range scan completed for {hosts.Count} hosts.");
            }
            catch (Exception ex)
            {
                return new Response<RangeScanResponse>(ResponseStatus.Failure, null, $"Range scan failed: {ex.Message}");
            }
        }

        private static IEnumerable<string> ExpandRange(string range)
        {
            if (range.Contains('/'))
            {
                return ExpandCidr(range);
            }

            if (range.Contains('-'))
            {
                return ExpandExplicitRange(range);
            }

            if (IPAddress.TryParse(range, out _))
            {
                return new[] { range };
            }

            throw new ArgumentException("Invalid IP range format.");
        }

        private static IEnumerable<string> ExpandExplicitRange(string range)
        {
            var parts = range.Split('-', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var startIp) || !IPAddress.TryParse(parts[1], out var endIp))
            {
                throw new ArgumentException("Invalid explicit IP range format.");
            }

            var start = ToUInt32(startIp);
            var end = ToUInt32(endIp);
            if (start > end)
            {
                throw new ArgumentException("Range start must be less than or equal to range end.");
            }

            for (uint current = start; current <= end; current++)
            {
                yield return FromUInt32(current).ToString();
            }
        }

        private static IEnumerable<string> ExpandCidr(string cidr)
        {
            var parts = cidr.Split('/', 2, StringSplitOptions.TrimEntries);
            if (parts.Length != 2 || !IPAddress.TryParse(parts[0], out var baseIp) || !int.TryParse(parts[1], out var prefixLength))
            {
                throw new ArgumentException("Invalid CIDR format.");
            }

            if (baseIp.AddressFamily != System.Net.Sockets.AddressFamily.InterNetwork || prefixLength < 0 || prefixLength > 32)
            {
                throw new ArgumentException("Only IPv4 CIDR ranges are supported.");
            }

            var baseAddress = ToUInt32(baseIp);
            uint mask = prefixLength == 0 ? 0u : uint.MaxValue << (32 - prefixLength);
            var network = baseAddress & mask;
            var broadcast = network | ~mask;

            for (uint current = network; current <= broadcast; current++)
            {
                yield return FromUInt32(current).ToString();
            }
        }

        private static uint ToUInt32(IPAddress ipAddress)
        {
            var bytes = ipAddress.GetAddressBytes();
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return BitConverter.ToUInt32(bytes, 0);
        }

        private static IPAddress FromUInt32(uint value)
        {
            var bytes = BitConverter.GetBytes(value);
            if (BitConverter.IsLittleEndian)
            {
                Array.Reverse(bytes);
            }
            return new IPAddress(bytes);
        }
    }
}
