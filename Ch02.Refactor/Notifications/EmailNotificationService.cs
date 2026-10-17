namespace EduHub.Ch02.Notifications;

public sealed class EmailNotificationService : INotificationService
{
    private readonly TextWriter _output;

    public EmailNotificationService(TextWriter output) => _output = output;

    public void Send(string to, string message) =>
        _output.WriteLine($"[EMAIL] to {to}: {message}");
}
