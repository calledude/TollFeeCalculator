using System.Globalization;

namespace Norion.Calendar;

public static class CalendarFactory
{
    public static ICalendar GetCalendar()
    {
        var culture = CultureInfo.CurrentCulture;
        return culture.Name switch
        {
            "sv-SE" => new SwedishCalendar(),
            _ => throw new InvalidOperationException($"No implementation found for culture {culture.Name}")
        };
    }
}
