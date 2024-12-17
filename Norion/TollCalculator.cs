using Norion.Calendar;
using Norion.FeeHandling;
using Norion.Vehicles;

namespace TollFeeCalculator;

public static class TollCalculator
{
    private static readonly IFeeHandler _feeCalculationChain = CreateFeeCalculationChain();

    /// <summary>
    /// Calculate the total toll fee for one day
    /// </summary>
    /// <param name="vehicle">The vehicle</param>
    /// <param name="dates">Date and time of all passes on one day</param>
    /// <returns>The total toll fee for that day.</returns>
    public static int GetTollFee(IVehicle vehicle, DateTimeOffset[] dates)
    {
        if (dates.Length == 0)
            return 0;

        // If the vehicle is a toll free vehicle, we can skip calculating the fees for all the dates.
        if (IsTollFreeVehicle(vehicle))
            return 0;

        // Assumption from original:
        // totalFee -= baseFee is inherently incorrect seeing as multiple passes can exist within an hour from the 'base'

        var orderedDates = dates.Order();
        var firstDate = orderedDates.First();

        var totalFee = orderedDates
            .GroupBy(date => (int)(date - firstDate).TotalHours)
            .Sum(hourWindow => hourWindow.Max(GetTollFee));

        return Math.Min(totalFee, 60);
    }

    /// <summary>
    /// Calculate the fee for a certain date and time.
    /// </summary>
    /// <param name="date">Date and time of a certain toll pass.</param>
    /// <returns>The fee associated with passing on the given date and time.</returns>
    public static int GetTollFee(DateTimeOffset date)
    {
        if (IsTollFreeDate(date))
            return 0;

        return _feeCalculationChain.CalculateFee(date);
    }

    private static bool IsTollFreeVehicle(IVehicle vehicle)
    {
        if (vehicle == null)
            return false;

        return vehicle.IsTollFree;
    }

    private static bool IsTollFreeDate(DateTimeOffset date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        var calendar = CalendarFactory.GetCalendar();
        return calendar.IsPublicHoliday(date);
    }

    private static IFeeHandler CreateFeeCalculationChain()
    {
        var start = new TimeWindowFeeHandler(new TimeOnly(6, 0), new TimeOnly(6, 29), 8);

        start
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(6, 30), new TimeOnly(6, 59), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(7, 0), new TimeOnly(7, 59), 18))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(8, 0), new TimeOnly(8, 29), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(8, 30), new TimeOnly(14, 59), 8))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(15, 0), new TimeOnly(15, 29), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(15, 0), new TimeOnly(16, 59), 18))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(17, 0), new TimeOnly(17, 59), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(18, 0), new TimeOnly(18, 29), 8));

        return start;
    }
}