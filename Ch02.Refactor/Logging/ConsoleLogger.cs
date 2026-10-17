namespace EduHub.Ch02.Logging;

public sealed class ConsoleLogger : IAppLogger
{
    public void Info(string message) => Console.WriteLine($"[LOG] {message}");
}
