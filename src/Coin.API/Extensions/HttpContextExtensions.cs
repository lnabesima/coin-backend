using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace Coin.API.Extensions;

public static class HttpContextExtensions
{
    public const string UserIdItemKey = "UserId";

    public static Guid GetUserId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.User.FindFirst(ClaimTypes.NameIdentifier) is { } claim &&
            Guid.TryParse(claim.Value, out Guid claimUserId))
        {
            return claimUserId;
        }

        if (context.Items.TryGetValue(UserIdItemKey, out var item) && item is Guid userId)
        {
            return userId;
        }

        throw new InvalidOperationException("User ID is not present in the current request context.");
    }

    public static Guid? TryGetUserId(this HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (context.User.FindFirst(ClaimTypes.NameIdentifier) is { } claim &&
            Guid.TryParse(claim.Value, out Guid claimUserId))
        {
            return claimUserId;
        }

        if (context.Items.TryGetValue(UserIdItemKey, out var item) && item is Guid userId)
        {
            return userId;
        }

        return null;
    }
}
