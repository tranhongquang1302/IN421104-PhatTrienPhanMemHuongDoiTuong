namespace EduHub.Ch02.Logging;

public sealed class FileLogger : IAppLogger
{
    private readonly string _filePath;

    public FileLogger(string filePath) => _filePath = filePath;

    public void Info(string message) =>
        File.AppendAllText(_filePath, $"[{DateTime.Now}] {message}{Environment.NewLine}");
}
