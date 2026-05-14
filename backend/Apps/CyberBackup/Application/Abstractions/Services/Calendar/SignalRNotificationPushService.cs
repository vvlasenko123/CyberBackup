using Application.Abstractions.Services.Calendar.Contracts;
using Application.Abstractions.Services.Calendar.Hubs;
using Application.DTO.Calendar;
using Microsoft.AspNetCore.SignalR;

namespace Application.Abstractions.Services.Calendar;

/// <summary>
/// SignalR сервис отправки уведомлений.
/// </summary>
public sealed class SignalRNotificationPushService : INotificationPushService
{
    private readonly IHubContext<NotificationHub> _hubContext;

    public SignalRNotificationPushService(IHubContext<NotificationHub> hubContext)
    {
        _hubContext = hubContext;
    }

    /// <inheritdoc />
    public async Task SendToUserAsync(Guid userId, NotificationMessageDto notification, CancellationToken cancellationToken)
    {
        await _hubContext.Clients
            .User(userId.ToString("D"))
            .SendAsync("NotificationReceived", notification, cancellationToken);
    }
}