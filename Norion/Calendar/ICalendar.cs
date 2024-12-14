namespace Norion.Calendar;

public interface ICalendar
{
    bool IsPublicHoliday(DateTimeOffset date);
}
