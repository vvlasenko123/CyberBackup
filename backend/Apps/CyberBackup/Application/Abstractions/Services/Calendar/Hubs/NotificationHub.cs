using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Application.Abstractions.Services.Calendar.Hubs;

/// <summary>
/// Hub уведомлений
/// </summary>
[Authorize]
public sealed class NotificationHub : Hub
{
}