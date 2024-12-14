namespace Norion.Calendar;

public class SwedishCalendar : ICalendar
{
    public bool IsPublicHoliday(DateTimeOffset date)
    {
        var month = (Month)date.Month;
        var day = date.Day;
        return (month, day) switch
        {
            (Month.January, 1) => true,

            // Possibly easter? Will fluctuate from year to year, might have held for 2013
            // Could call an API here or implement an easter date calculator
            // ... Google how and you'll see why I chose not to :)
            (Month.March, 28) => true,
            (Month.March, 29) => true,

            (Month.April, 1) => true,
            (Month.April, 30) => true,

            (Month.May, 1) => true,
            (Month.May, 8) => true,
            (Month.May, 9) => true,

            (Month.June, 5) => true,
            (Month.June, 6) => true,
            (Month.June, 21) => true,

            (Month.July, _) => true,

            (Month.November, 1) => true,

            (Month.December, 24) => true,
            (Month.December, 25) => true,
            (Month.December, 26) => true,
            (Month.December, 31) => true,

            _ => false,
        };
    }
}
