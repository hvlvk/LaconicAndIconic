using System.Globalization;
using System.Security.Claims;

namespace LaconicAndIconic.Web.Extensions;

public static class ClaimsPrincipalExtension
{
    public static int GetUserId(this ClaimsPrincipal claimsPrincipal)
    {
        var userIdString = claimsPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString))
        {
            return 0;
        }

        return int.Parse(userIdString, CultureInfo.InvariantCulture);
    }
}
