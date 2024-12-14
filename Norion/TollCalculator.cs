using Norion.Calendar;
using Norion.FeeHandling;
using Norion.Vehicles;

namespace TollFeeCalculator;

public static class TollCalculator
{
    private static readonly IFeeHandler _feeCalculationChain =
        new TimeWindowFeeHandler(new TimeOnly(6, 0), new TimeOnly(6, 29), 8)
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(6, 30), new TimeOnly(6, 59), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(7, 0), new TimeOnly(7, 59), 18))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(8, 0), new TimeOnly(8, 29), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(8, 30), new TimeOnly(14, 59), 8))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(15, 0), new TimeOnly(15, 29), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(15, 0), new TimeOnly(16, 59), 18))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(17, 0), new TimeOnly(17, 59), 13))
            .SetNext(new TimeWindowFeeHandler(new TimeOnly(18, 0), new TimeOnly(18, 29), 8));

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

        var intervalStart = dates[0];
        var totalFee = 0;
        var baseFee = GetTollFee(intervalStart);
        foreach (var date in dates)
        {
            var nextFee = GetTollFee(date);

            var diff = date - intervalStart;
            if (diff.TotalMinutes <= 60)
            {
                if (totalFee > 0)
                    totalFee -= baseFee;

                totalFee += Math.Max(nextFee, baseFee);
            }
            else
            {
                totalFee += nextFee;
            }
        }

        return Math.Min(totalFee, 60);
    }

    private static bool IsTollFreeVehicle(IVehicle vehicle)
    {
        if (vehicle == null)
            return false;

        return vehicle.IsTollFree;
    }

    public static int GetTollFee(DateTimeOffset date)
    {
        if (IsTollFreeDate(date))
            return 0;

        return _feeCalculationChain.CalculateFee(date);
    }

    private static bool IsTollFreeDate(DateTimeOffset date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        var calendar = CalendarFactory.GetCalendar();
        return calendar.IsPublicHoliday(date);
    }
}