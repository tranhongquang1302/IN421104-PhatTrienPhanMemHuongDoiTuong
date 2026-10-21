namespace EduHub.Ch02.Notifications;

public sealed class SmsNotificationService : INotificationService
{
    private readonly TextWriter _output;

    public SmsNotificationService(TextWriter output) => _output = output;

    public void Send(string to, string message) =>
        _output.WriteLine($"[SMS] to {to}: {message}");
}
