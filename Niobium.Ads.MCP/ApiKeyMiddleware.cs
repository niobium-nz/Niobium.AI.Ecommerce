namespace Niobium.Ads.MCP
{
    public class ApiKeyMiddleware(RequestDelegate next)
    {
        private const string API_KEY_HEADER_NAME = "X-Api-Key"; // The expected header name

        public async Task InvokeAsync(HttpContext context, IConfiguration configuration)
        {
            var apikey = Environment.GetEnvironmentVariable("X_API_KEY");
            if (apikey == null)
            {
                context.Response.StatusCode = 500; // Unauthorized
                await context.Response.WriteAsync("API Key is not configured.");
                return;
            }

            string? requestAPIKey = null;
            if (context.Request.Headers.TryGetValue(API_KEY_HEADER_NAME, out var extractedApiKey))
            {
                requestAPIKey = extractedApiKey;
            }
            else if (!string.IsNullOrEmpty(context.Request.Headers.Authorization))
            {
                requestAPIKey = context.Request.Headers.Authorization;
            }

            if (string.IsNullOrEmpty(requestAPIKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("API Key was not provided.");
                return;
            }

            if (!apikey.Equals(requestAPIKey))
            {
                context.Response.StatusCode = 401; // Unauthorized
                await context.Response.WriteAsync("Unauthorized client.");
                return;
            }

            await next(context);
        }
    }
}
