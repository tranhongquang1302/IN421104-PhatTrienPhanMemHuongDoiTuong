namespace EduHub.Ch02.Patterns.Strategies;

public sealed class CsvReportStrategy : IReportStrategy
{
    public string Name => "CSV";

    public string Render(IReadOnlyList<ClubStat> stats)
    {
        var lines = new List<string> { "Club,Members,Events,Registered,CheckedIn" };
        foreach (var stat in stats)
        {
            var clubName = "\"" + stat.ClubName.Replace("\"", "\"\"") + "\"";
            lines.Add(FormattableString.Invariant(
                $"{clubName},{stat.Members},{stat.Events},{stat.Registered},{stat.CheckedIn}"));
        }

        return string.Join(Environment.NewLine, lines);
    }
}
