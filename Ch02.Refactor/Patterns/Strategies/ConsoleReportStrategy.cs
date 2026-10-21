namespace EduHub.Ch02.Patterns.Strategies;

public sealed class ConsoleReportStrategy : IReportStrategy
{
    public string Name => "Console";

    public string Render(IReadOnlyList<ClubStat> stats) =>
        string.Join(Environment.NewLine, stats.Select(stat =>
            $"Club: {stat.ClubName} | Members: {stat.Members} | Events: {stat.Events}" +
            $" | Registered: {stat.Registered} | CheckedIn: {stat.CheckedIn}"));
}
