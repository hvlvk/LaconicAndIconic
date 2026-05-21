namespace LaconicAndIconic.Web.Models;

public sealed record NotificationDto(
    string Title,
    string Message,
    string Type,
    DateTimeOffset CreatedAt);
