using Qoip.ZeroTrustNetwork.Common;
using System.Diagnostics;
using System.Net;

namespace Qoip.ZeroTrustNetwork.NetworkConnectivity
{
    public class HttpProbeRequest
    {
        public string Url { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }

        public HttpProbeRequest(string url, int timeout, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(url);

            Url = url;
            Timeout = timeout;
            DetailLevel = detailLevel;
        }

        public Response<HttpProbeResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<HttpProbeResponse>> ExecuteAsync()
        {
            try
            {
                if (!Uri.TryCreate(Url, UriKind.Absolute, out var requestUri) ||
                    (requestUri.Scheme != Uri.UriSchemeHttp && requestUri.Scheme != Uri.UriSchemeHttps))
                {
                    return new Response<HttpProbeResponse>(ResponseStatus.Failure, null, "Invalid HTTP or HTTPS URL.");
                }

                using var handler = new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    AutomaticDecompression = DecompressionMethods.All
                };
                using var client = new HttpClient(handler)
                {
                    Timeout = TimeSpan.FromMilliseconds(Timeout)
                };

                var redirectChain = new List<string>();
                var stopwatch = Stopwatch.StartNew();
                using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
                request.Headers.TryAddWithoutValidation("User-Agent", "Qoip-NetTest HttpProbeRequest");

                using var response = await SendWithRedirectsAsync(client, requestUri, redirectChain);
                stopwatch.Stop();

                var probeResponse = new HttpProbeResponse
                {
                    Url = Url,
                    FinalUrl = response.RequestMessage?.RequestUri?.ToString() ?? requestUri.ToString(),
                    StatusCode = (int)response.StatusCode,
                    ReasonPhrase = response.ReasonPhrase ?? string.Empty,
                    IsSuccessStatusCode = response.IsSuccessStatusCode,
                    IsHttps = string.Equals(response.RequestMessage?.RequestUri?.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase),
                    Server = response.Headers.Server.ToString(),
                    ContentType = response.Content.Headers.ContentType?.ToString() ?? string.Empty,
                    DetailLevel = DetailLevel,
                    TotalResponseTime = stopwatch.ElapsedMilliseconds,
                    RedirectChain = redirectChain,
                    Headers = BuildHeaders(response)
                };

                var message = $"HTTP probe completed with status code {probeResponse.StatusCode}.";
                if (DetailLevel == DetailLevel.Info || DetailLevel == DetailLevel.Debug)
                {
                    message += $" Final URL: {probeResponse.FinalUrl}.";
                }

                return new Response<HttpProbeResponse>(ResponseStatus.Ok, probeResponse, message);
            }
            catch (Exception ex)
            {
                return new Response<HttpProbeResponse>(ResponseStatus.Failure, null, $"HTTP probe failed: {ex.Message}");
            }
        }

        private static async Task<HttpResponseMessage> SendWithRedirectsAsync(HttpClient client, Uri requestUri, List<string> redirectChain)
        {
            Uri currentUri = requestUri;

            for (var redirectCount = 0; redirectCount < 10; redirectCount++)
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, currentUri);
                request.Headers.TryAddWithoutValidation("User-Agent", "Qoip-NetTest HttpProbeRequest");

                var response = await client.SendAsync(request);
                redirectChain.Add(currentUri.ToString());

                if (!IsRedirect(response.StatusCode) || response.Headers.Location == null)
                {
                    return response;
                }

                currentUri = response.Headers.Location.IsAbsoluteUri
                    ? response.Headers.Location
                    : new Uri(currentUri, response.Headers.Location);
                response.Dispose();
            }

            throw new InvalidOperationException("Too many HTTP redirects encountered.");
        }

        private static bool IsRedirect(HttpStatusCode statusCode)
        {
            return statusCode == HttpStatusCode.Moved ||
                   statusCode == HttpStatusCode.Redirect ||
                   statusCode == HttpStatusCode.RedirectMethod ||
                   statusCode == HttpStatusCode.TemporaryRedirect ||
                   statusCode == HttpStatusCode.PermanentRedirect;
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
    }
}
