using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using System.Collections.Concurrent;

namespace LaconicAndIconic.Web.Filters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, AllowMultiple = false)]
public class RateLimitingAttribute : ActionFilterAttribute
{
    private readonly int _maxRequestsPerMinute;

    public RateLimitingAttribute(int maxRequestsPerMinute = 10)
    {
        _maxRequestsPerMinute = maxRequestsPerMinute;
    }

    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var cache = context.HttpContext.RequestServices.GetService<IMemoryCache>();
        if (cache == null)
        {
            base.OnActionExecuting(context);
            return;
        }

        var ipAddress = context.HttpContext.Connection.RemoteIpAddress?.ToString();
        if (string.IsNullOrEmpty(ipAddress))
        {
            base.OnActionExecuting(context);
            return;
        }

        var key = $"RateLimit_{ipAddress}_{context.ActionDescriptor.Id}";

        if (!cache.TryGetValue(key, out RateLimitInfo? info) || info == null)
        {
            info = new RateLimitInfo { Count = 1 };
            cache.Set(key, info, TimeSpan.FromMinutes(1));
        }
        else
        {
            lock (info)
            {
                info.Count++;
            }

            if (info.Count > _maxRequestsPerMinute)
            {
                context.Result = new RedirectToActionResult("RateLimitError", "Home", null);
                return;
            }
        }

        base.OnActionExecuting(context);
    }
}

public record RateLimitInfo
{
    public int Count { get; set; }
}
