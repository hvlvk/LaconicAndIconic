using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace LaconicAndIconic.Web.Hubs;

[Authorize]
public sealed class NotificationsHub : Hub
{
}
