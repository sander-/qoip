using Qoip.ZeroTrustNetwork.Common;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Qoip.ZeroTrustNetwork.NetworkSecurity
{
    public class TlsHandshakeAnalysisRequest
    {
        public string Host { get; }
        public int Port { get; }
        public int Timeout { get; }
        public DetailLevel DetailLevel { get; }

        public TlsHandshakeAnalysisRequest(string host, int port, int timeout, DetailLevel detailLevel)
        {
            ArgumentNullException.ThrowIfNull(host);

            Host = host;
            Port = port;
            Timeout = timeout;
            DetailLevel = detailLevel;
        }

        public Response<TlsHandshakeAnalysisResponse> Execute()
        {
            return ExecuteAsync().GetAwaiter().GetResult();
        }

        public async Task<Response<TlsHandshakeAnalysisResponse>> ExecuteAsync()
        {
            try
            {
                using var tcpClient = new TcpClient();
                using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(Timeout));
                await tcpClient.ConnectAsync(Host, Port, cts.Token);
                using var networkStream = tcpClient.GetStream();
                using var sslStream = new SslStream(networkStream, false, (_, _, _, _) => true);

                await sslStream.AuthenticateAsClientAsync(new SslClientAuthenticationOptions
                {
                    TargetHost = Host,
                    EnabledSslProtocols = SslProtocols.None,
                    CertificateRevocationCheckMode = X509RevocationMode.NoCheck
                }, cts.Token);

                var remoteCertificate = sslStream.RemoteCertificate != null
                    ? new X509Certificate2(sslStream.RemoteCertificate)
                    : null;

                var response = new TlsHandshakeAnalysisResponse
                {
                    Host = Host,
                    Port = Port,
                    DetailLevel = DetailLevel,
                    HandshakeSucceeded = true,
                    NegotiatedSslProtocol = sslStream.SslProtocol.ToString(),
                    CipherAlgorithm = sslStream.CipherAlgorithm.ToString(),
                    CipherStrength = sslStream.CipherStrength,
                    HashAlgorithm = sslStream.HashAlgorithm.ToString(),
                    HashStrength = sslStream.HashStrength,
                    KeyExchangeAlgorithm = sslStream.KeyExchangeAlgorithm.ToString(),
                    KeyExchangeStrength = sslStream.KeyExchangeStrength,
                    CertificateSubject = remoteCertificate?.Subject ?? string.Empty,
                    CertificateIssuer = remoteCertificate?.Issuer ?? string.Empty,
                    CertificateValidFrom = remoteCertificate?.NotBefore ?? DateTime.MinValue,
                    CertificateValidTo = remoteCertificate?.NotAfter ?? DateTime.MinValue
                };

                if (response.CertificateValidTo != DateTime.MinValue && response.CertificateValidTo <= DateTime.UtcNow)
                {
                    response.Findings.Add("The remote certificate is expired.");
                }

                if (response.NegotiatedSslProtocol.Contains("Ssl3", StringComparison.OrdinalIgnoreCase) ||
                    response.NegotiatedSslProtocol.Contains("Tls", StringComparison.OrdinalIgnoreCase) &&
                    (response.NegotiatedSslProtocol.Equals("Tls", StringComparison.OrdinalIgnoreCase) ||
                     response.NegotiatedSslProtocol.Equals("Tls11", StringComparison.OrdinalIgnoreCase)))
                {
                    response.Findings.Add($"Legacy TLS protocol negotiated: {response.NegotiatedSslProtocol}.");
                }

                var status = response.Findings.Count == 0 ? ResponseStatus.Ok : ResponseStatus.Warning;
                var message = response.HandshakeSucceeded
                    ? $"TLS handshake analysis completed using {response.NegotiatedSslProtocol}."
                    : "TLS handshake analysis failed.";

                return new Response<TlsHandshakeAnalysisResponse>(status, response, message);
            }
            catch (Exception ex)
            {
                return new Response<TlsHandshakeAnalysisResponse>(ResponseStatus.Failure, null, $"TLS handshake analysis failed: {ex.Message}");
            }
        }
    }
}
