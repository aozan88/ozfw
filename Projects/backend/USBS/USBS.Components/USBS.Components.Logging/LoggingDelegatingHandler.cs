using OzFwLogging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;


namespace USBS.Components.Logging
{
    using OzFwLogging; // Make sure this namespace is included

    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> _logger;
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IKonLogger _konLogger; // Optional dependency
        private readonly bool _hasKonLogger;

        public LoggingDelegatingHandler(
            ILogger<LoggingDelegatingHandler> logger,
            IHttpContextAccessor contextAccessor,
            IServiceProvider serviceProvider) // Use service provider instead
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _contextAccessor = contextAccessor ?? throw new ArgumentNullException(nameof(contextAccessor));

            // Try to get the IKonLogger service but don't fail if not available
            try
            {
                _konLogger = serviceProvider.GetService(typeof(IKonLogger)) as IKonLogger;
                _hasKonLogger = _konLogger != null;
            }
            catch
            {
                _hasKonLogger = false;
            }
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                // Add token from session if available
                var httpContext = _contextAccessor.HttpContext;
                if (httpContext?.Session != null)
                {
                    var token = httpContext.Session.GetString("Token");
                    if (!string.IsNullOrEmpty(token))
                    {
                        request.Headers.Add("Authorization", $"Bearer {token}");
                    }
                }

                // Log request
                _logger.LogInformation("Sending request to {Url}", request.RequestUri);

                // Use KonLogger if available
                if (_hasKonLogger)
                {
                    _konLogger.LogInformation($"HTTP Request: {request.Method} {request.RequestUri}");
                }

                // Execute the request
                var response = await base.SendAsync(request, cancellationToken);

                // Log response
                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation("Received success response from {Url}: {StatusCode}",
                        response.RequestMessage.RequestUri, (int)response.StatusCode);

                    if (_hasKonLogger)
                    {
                        _konLogger.LogInformation($"HTTP Response: {(int)response.StatusCode} from {response.RequestMessage.RequestUri}");
                    }
                }
                else
                {
                    _logger.LogWarning("Received non-success response from {Url}: {StatusCode}",
                        response.RequestMessage.RequestUri, (int)response.StatusCode);

                    if (_hasKonLogger)
                    {
                        _konLogger.LogWarning($"HTTP Error Response: {(int)response.StatusCode} from {response.RequestMessage.RequestUri}");
                    }
                }

                return response;
            }
            catch (HttpRequestException ex) when (ex.InnerException is SocketException se &&
                                                 se.SocketErrorCode == SocketError.ConnectionRefused)
            {
                var hostWithPort = request.RequestUri.IsDefaultPort
                    ? request.RequestUri.DnsSafeHost
                    : $"{request.RequestUri.DnsSafeHost}:{request.RequestUri.Port}";

                _logger.LogCritical(ex, "Connection failed to {Host}. Check service configuration.", hostWithPort);

                if (_hasKonLogger)
                {
                    _konLogger.LogCritical($"Connection failed to {hostWithPort}: {ex.Message}");
                }

                return new HttpResponseMessage(HttpStatusCode.BadGateway)
                {
                    RequestMessage = request
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending HTTP request to {Url}", request.RequestUri);

                if (_hasKonLogger)
                {
                    _konLogger.LogError($"HTTP Request Error: {ex.Message}");
                }

                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                {
                    RequestMessage = request
                };
            }
        }
    }
}
