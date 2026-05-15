using Qoip.ZeroTrustNetwork.Common;
using System.Net;

namespace Qoip.ZeroTrustNetwork.NetworkSecurity
{
    public class SecurityHeaderAnalysisRequest
    {
        private static readonly string[] RequiredHeaders =
        [
            "Strict-Transport-Security",
            "Content-Security-Policy",
            "X-Frame-Options",
            "X-Content-Type-Options",
            "Referrer-Policy",
            "Permissions-Policy"
        ];

        public string Url { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }

        public SecurityHeaderAnalysisRequest(string url, int timeout, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(url);

            Url = url;
            Timeout = timeout;
            DetailLevel = detailLevel;
        }

        public Response<SecurityHeaderAnalysisResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<SecurityHeaderAnalysisResponse>> ExecuteAsync()
        {
            try
            {
                if (!Uri.TryCreate(Url, UriKind.Absolute, out var requestUri) ||
                    (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps))
                {
                    return new Response<SecurityHeaderAnalysisResponse>(ResponseStatus.Failure, null, "Invalid HTTP or HTTPS URL.");
                }

                using var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = true,
                    AutomaticDecompression = DecompressionMethods.All
                };
                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Timeout)
                };
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.TryAddWithoutValidation("User-Agent", "Qoip-NetTest SecurityHeaderAnalysisRequest");

                using var response = await client.SendAsync(request);
                var headers = BuildHeaders(response);
                var analysis = BuildAnalysis(headers);
                analysis.Url = response.RequestMessage?.RequestUri?.ToString() ?? Url;
                analysis.DetailLevel = DetailLevel;

                var status = analysis.MissingHeaders.Count == 0 ? ResponseStatus.Ok : ResponseStatus.Warning;
                var message = analysis.MissingHeaders.Count == 0
                    ? "Security header analysis completed. All expected headers were found."
                    : $"Security header analysis completed. Missing headers: {string.Join(", ", analysis.MissingHeaders)}.";

                return new Response<SecurityHeaderAnalysisResponse>(status, analysis, message);
            }
            catch (Exception ex)
            {
                return new Response<SecurityHeaderAnalysisResponse>(ResponseStatus.Failure, null, $"Security header analysis failed: {ex.Message}");
            }
        }

        private static Dictionary<string, string> BuildHeaders(HttpResponseMessage response)
        {
            var headers = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var header in response.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }

            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(", ", header.Value);
            }

            return headers;
        }

        private static SecurityHeaderAnalysisResponse BuildAnalysis(Dictionary<string, string> headers)
        {
            var response = new SecurityHeaderAnalysisResponse
            {
                PresentHeaders = headers
                    .Where(kv => RequiredHeaders.Contains(kv.Key, StringComparer.OrdinalIgnoreCase))
                    .ToDictionary(kv => kv.Key, kv => kv.Value, StringComparer.OrdinalIgnoreCase)
            };

            response.HasStrictTransportSecurity = response.PresentHeaders.ContainsKey("Strict-Transport-Security");
            response.HasContentSecurityPolicy = response.PresentHeaders.ContainsKey("Content-Security-Policy");
            response.HasXFrameOptions = response.PresentHeaders.ContainsKey("X-Frame-Options");
            response.HasXContentTypeOptions = response.PresentHeaders.ContainsKey("X-Content-Type-Options");
            response.HasReferrerPolicy = response.PresentHeaders.ContainsKey("Referrer-Policy");
            response.HasPermissionsPolicy = response.PresentHeaders.ContainsKey("Permissions-Policy");

            foreach (var header in RequiredHeaders)
            {
                if (!response.PresentHeaders.ContainsKey(header))
                {
                    response.MissingHeaders.Add(header);
                    response.Findings.Add($"Missing header: {header}");
                }
            }

            if (response.PresentHeaders.TryGetValue("Strict-Transport-Security", out var hstsValue) && string.IsNullOrWhiteSpace(hstsValue))
            {
                response.Findings.Add("Strict-Transport-Security header is present but empty.");
            }

            if (response.PresentHeaders.TryGetValue("Content-Security-Policy", out var cspValue) && string.IsNullOrWhiteSpace(cspValue))
            {
                response.Findings.Add("Content-Security-Policy header is present but empty.");
            }

            return response;
        }
    }
}
