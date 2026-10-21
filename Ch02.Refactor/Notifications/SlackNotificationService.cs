namespace EduHub.Ch02.Notifications;

public sealed class SlackNotificationService : INotificationService
{
    private readonly TextWriter _output;

    public SlackNotificationService(TextWriter output) => _output = output;

    public void Send(string to, string message) =>
        _output.WriteLine($"[SLACK] to {to}: {message}");
}
