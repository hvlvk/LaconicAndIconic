using System.Diagnostics;

namespace LaconicAndIconic.Web.Middleware;

public sealed class ExecutionTimeMiddleware(
    RequestDelegate next,
    ILogger<ExecutionTimeMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!logger.IsEnabled(LogLevel.Information))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();

            var request = context.Request;

            logger.LogInformation(
                "HTTP request completed: {Method} {Url} in {ElapsedMilliseconds} ms",
                request.Method,
                GetRequestUrl(request),
                stopwatch.ElapsedMilliseconds);
        }
    }

    private static string GetRequestUrl(HttpRequest request)
    {
        return request.QueryString.HasValue
            ? $"{request.Path}{request.QueryString}"
            : request.Path.ToString();
    }
}
