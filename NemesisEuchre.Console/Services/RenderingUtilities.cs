using System.Globalization;

using Humanizer;

using Spectre.Console;

namespace NemesisEuchre.Console.Services;

public static class RenderingUtilities
{
    public static Table CreateStyledTable(string? title = null)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey);

        if (title is not null)
        {
            table.Title(title);
        }

        return table;
    }

    public static string FormatNullableMetric(double? value, string format = "F4")
    {
        return value.HasValue ? value.Value.ToString(format, CultureInfo.InvariantCulture) : "[dim]-[/]";
    }

    public static string FormatNullableDuration(TimeSpan? duration)
    {
        return duration.HasValue
            ? duration.Value.Humanize(2, countEmptyUnits: true, minUnit: TimeUnit.Second)
            : "[dim]-[/]";
    }
}
