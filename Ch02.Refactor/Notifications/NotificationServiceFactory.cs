namespace EduHub.Ch02.Notifications;

public sealed class NotificationServiceFactory : INotificationServiceFactory
{
    private readonly TextWriter _output;

    public NotificationServiceFactory(TextWriter output) => _output = output;

    public INotificationService Create(NotificationChannel channel) => channel switch
    {
        NotificationChannel.Email => new EmailNotificationService(_output),
        NotificationChannel.Sms => new SmsNotificationService(_output),
        NotificationChannel.Slack => new SlackNotificationService(_output),
        _ => throw new ArgumentOutOfRangeException(nameof(channel), channel, "Unknown notification channel")
    };
}
