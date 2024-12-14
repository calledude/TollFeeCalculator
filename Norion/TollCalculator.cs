using Norion;
using Norion.Calendar;
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
    public static int GetTollFee(IVehicle vehicle, DateTime[] dates)
    {
        if (dates.Length == 0)
            return 0;

        // If the vehicle is a toll free vehicle, we can skip calculating the fees for all the dates.
        if (IsTollFreeVehicle(vehicle))
            return 0;

        var intervalStart = dates[0];
        var totalFee = 0;
        var baseFee = GetTollFee(intervalStart, vehicle);
        foreach (var date in dates)
        {
            var nextFee = GetTollFee(date, vehicle);

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

    public static int GetTollFee(DateTime date, IVehicle vehicle)
    {
        if (IsTollFreeDate(date))
            return 0;

        return _feeCalculationChain.CalculateFee(date);
    }

    private static bool IsTollFreeDate(DateTime date)
    {
        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        var month = (Month)date.Month;
        var day = date.Day;

        // These dates are all very clearly holiday dates
        // Just noting that down here since holidays aren't... Universal.
        // So while this switch might hold for, say, Sweden, it might be completely wrong for another country.
        // Norway for example has 17th of May as a national holiday rather than the equivalent 6th of June in Sweden.
        // Possibly revise this by having an implementation of 'NationalCalendar' that holds information of such holidays instead.
        //
        // return calendar.IsPublicHoliday(date);
        return (month, day) switch
        {
            (Month.January, 1) => true,

            // Possibly easter? Will fluctuate from year to year, might have held for 2013
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