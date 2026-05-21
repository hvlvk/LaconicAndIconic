using System.Globalization;
using LaconicAndIconic.DAL.Data;
using LaconicAndIconic.Web.Hubs;
using LaconicAndIconic.Web.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace LaconicAndIconic.Web.Services;

public sealed class NotificationBackgroundService(
    IServiceScopeFactory scopeFactory,
    IHubContext<NotificationsHub> hubContext,
    ILogger<NotificationBackgroundService> logger) : BackgroundService
{
    private static readonly TimeSpan CheckInterval = TimeSpan.FromSeconds(15);
    private DateTime _lastCheckedAt = DateTime.UtcNow;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(CheckInterval);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckDatabaseEventsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
#pragma warning disable CA1031 // Keep the hosted service alive when one notification check fails.
            catch (Exception ex)
#pragma warning restore CA1031
            {
                logger.LogError(ex, "Failed to process notification check");
            }

            if (!await timer.WaitForNextTickAsync(stoppingToken))
            {
                break;
            }
        }
    }

    private async Task CheckDatabaseEventsAsync(CancellationToken cancellationToken)
    {
        var checkedAt = DateTime.UtcNow;
        await using var scope = scopeFactory.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var recipeCount = await dbContext.Recipes.CountAsync(cancellationToken);
        var userIds = await dbContext.Users
            .Select(user => user.Id.ToString(CultureInfo.InvariantCulture))
            .ToListAsync(cancellationToken);

        if (userIds.Count == 0)
        {
            logger.LogInformation("Notification check completed, but no users were found");
            return;
        }

        var notification = new NotificationDto(
            "Нотифікація",
            $"BackgroundService перевірив базу даних. Рецептів у системі: {recipeCount}.",
            "info",
            DateTimeOffset.UtcNow);

        /*await hubContext.Clients.Users(userIds)
            .SendAsync("ReceiveNotification", notification, cancellationToken);*/

        logger.LogInformation(
            "Notification was sent via SignalR to {UserCount} users",
            userIds.Count);

        await SendNewCommentNotificationAsync(dbContext, userIds, checkedAt, cancellationToken);
        await SendNewRatingNotificationAsync(dbContext, userIds, checkedAt, cancellationToken);

        _lastCheckedAt = checkedAt;
    }

    private async Task SendNewCommentNotificationAsync(
        ApplicationDbContext dbContext,
        List<string> userIds,
        DateTime checkedAt,
        CancellationToken cancellationToken)
    {
        var commentsCount = await dbContext.Comments
            .AsNoTracking()
            .CountAsync(
                comment => comment.CreatedAt > _lastCheckedAt && comment.CreatedAt <= checkedAt,
                cancellationToken);

        if (commentsCount == 0)
        {
            return;
        }

        var notification = new NotificationDto(
            "Нові коментарі",
            $"У системі з'явилися нові коментарі: {commentsCount}.",
            "comments-created",
            DateTimeOffset.UtcNow);

        await hubContext.Clients.Users(userIds)
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }

    private async Task SendNewRatingNotificationAsync(
        ApplicationDbContext dbContext,
        List<string> userIds,
        DateTime checkedAt,
        CancellationToken cancellationToken)
    {
        var ratingsCount = await dbContext.Ratings
            .AsNoTracking()
            .CountAsync(
                rating => rating.CreatedAt > _lastCheckedAt && rating.CreatedAt <= checkedAt,
                cancellationToken);

        if (ratingsCount == 0)
        {
            return;
        }

        var notification = new NotificationDto(
            "Нові оцінки",
            $"У системі з'явилися нові оцінки рецептів: {ratingsCount}.",
            "ratings-created",
            DateTimeOffset.UtcNow);

        await hubContext.Clients.Users(userIds)
            .SendAsync("ReceiveNotification", notification, cancellationToken);
    }
}
