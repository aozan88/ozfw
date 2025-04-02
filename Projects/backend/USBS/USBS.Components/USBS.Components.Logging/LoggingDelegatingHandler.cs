using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Sockets;


namespace USBS.Components.Logging
{
    public class LoggingDelegatingHandler : DelegatingHandler
    {
        private readonly ILogger<LoggingDelegatingHandler> logger;
        private readonly IHttpContextAccessor _context;


        public LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger, IHttpContextAccessor context)
        {
            this.logger = logger;
            _context = context;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                var token = _context.HttpContext?.Session.GetString("Token");
                if (!string.IsNullOrEmpty(token))
                {
                    request.Headers.Add("Authorization", "Bearer " + token);

                }

                logger.LogInformation("Sending request to {Url}", request.RequestUri);

                var response = await base.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    logger.LogInformation("Received a success response from {Url}", response.RequestMessage.RequestUri);
                }
                else
                {
                    logger.LogWarning("Received a non-success status code {StatusCode} from {Url}",
                        (int)response.StatusCode, response.RequestMessage.RequestUri);
                }

                return response;
            }
            catch (HttpRequestException ex)
                when (ex.InnerException is SocketException se && se.SocketErrorCode == SocketError.ConnectionRefused)
            {
                var hostWithPort = request.RequestUri.IsDefaultPort
                    ? request.RequestUri.DnsSafeHost
                    : $"{request.RequestUri.DnsSafeHost}:{request.RequestUri.Port}";

                logger.LogCritical(ex, "Unable to connect to {Host}. Please check the " +
                                        "configuration to ensure the correct URL for the service " +
                                        "has been configured.", hostWithPort);
            }

            return new HttpResponseMessage(HttpStatusCode.BadGateway)
            {
                RequestMessage = request
            };
        }
    }
}
