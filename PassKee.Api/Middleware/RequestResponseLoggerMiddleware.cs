namespace PassKee.Api.Middleware
{
    public class RequestResponseLoggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestResponseLoggerMiddleware> _logger;
        
        public RequestResponseLoggerMiddleware(RequestDelegate next, IConfiguration config, ILogger<RequestResponseLoggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext httpContext)
        {
            // Middleware is enabled only when the EnableRequestResponseLogging config value is set.
            _logger.LogDebug($"HTTP request information:\n" +
                $"\tMethod: {httpContext.Request.Method}\n" +
                $"\tPath: {httpContext.Request.Path}\n" +
                $"\tQueryString: {httpContext.Request.QueryString}\n" +
                $"\tHeaders: {FormatHeaders(httpContext.Request.Headers)}\n" +
                $"\tSchema: {httpContext.Request.Scheme}\n" +
                $"\tHost: {httpContext.Request.Host}\n" +
                $"\tBody: {await ReadBodyFromRequest(httpContext.Request)}");
            await _next(httpContext);
        }

        private static string FormatHeaders(IHeaderDictionary headers) => string.Join(", ", headers.Select(kvp => $"{{{kvp.Key}: {string.Join(", ", kvp.Value)}}}"));

        private static async Task<string> ReadBodyFromRequest(HttpRequest request)
        {
            // Ensure the request's body can be read multiple times (for the next middlewares in the pipeline).
            request.EnableBuffering();

            using var streamReader = new StreamReader(request.Body, leaveOpen: true);
            var requestBody = await streamReader.ReadToEndAsync();

            // Reset the request's body stream position for next middleware in the pipeline.
            request.Body.Position = 0;
            return requestBody;
        }
    }
}
