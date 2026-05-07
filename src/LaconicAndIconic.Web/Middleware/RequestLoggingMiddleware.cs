using System.Security.Claims;
using System.Text;

namespace LaconicAndIconic.Web.Middleware;

public sealed class RequestLoggingMiddleware(
    RequestDelegate next,
    ILogger<RequestLoggingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (!logger.IsEnabled(LogLevel.Information))
        {
            await next(context);
            return;
        }

        var request = context.Request;
        var requestBody = await ReadRequestBodyAsync(request, context.RequestAborted);
        var headers = FormatHeaders(request.Headers);
        var userId = GetCurrentUserId(context.User);

        logger.LogInformation(
            "Incoming HTTP request: {Method} {Url}. ClientIp: {ClientIp}. UserId: {UserId}. Headers: {Headers}. Body: {Body}",
            request.Method,
            GetRequestUrl(request),
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            userId ?? "anonymous",
            headers,
            string.IsNullOrWhiteSpace(requestBody) ? "<empty>" : requestBody);

        await next(context);
    }

    private static async Task<string> ReadRequestBodyAsync(HttpRequest request, CancellationToken cancellationToken)
    {
        if (!HttpMethodsCanHaveBody(request.Method) || request.ContentLength is null or 0)
        {
            return string.Empty;
        }

        request.EnableBuffering();

        using var reader = new StreamReader(
            request.Body,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var body = await reader.ReadToEndAsync(cancellationToken);

        request.Body.Position = 0;

        return body;
    }

    private static string FormatHeaders(IHeaderDictionary headers)
    {
        if (headers.Count == 0)
        {
            return "<none>";
        }

        var builder = new StringBuilder();

        foreach (var header in headers)
        {
            if (builder.Length > 0)
            {
                builder.Append("; ");
            }

            builder.Append(header.Key)
                .Append(": ")
                .Append(header.Value.ToString());
        }

        return builder.ToString();
    }

    private static string GetRequestUrl(HttpRequest request)
    {
        return request.QueryString.HasValue
            ? $"{request.Path}{request.QueryString}"
            : request.Path.ToString();
    }

    private static string? GetCurrentUserId(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true
            ? user.FindFirstValue(ClaimTypes.NameIdentifier)
            : null;
    }

    private static bool HttpMethodsCanHaveBody(string method)
    {
        return HttpMethods.IsPost(method)
            || HttpMethods.IsPut(method)
            || HttpMethods.IsPatch(method)
            || HttpMethods.IsDelete(method);
    }
}
