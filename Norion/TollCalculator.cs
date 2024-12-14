using Norion;
using Norion.Vehicles;
using System.Diagnostics.CodeAnalysis;

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
        var month = date.Month;
        var day = date.Day;

        if (date.DayOfWeek == DayOfWeek.Saturday || date.DayOfWeek == DayOfWeek.Sunday)
            return true;

        if (month is 1 && day is 1)
            return true;

        if (month is 3 && (day is 28 or 29))
            return true;

        if (month is 4 && (day is 1 or 30))
            return true;

        if (month is 5 && (day is 1 or 8 or 9))
            return true;

        if (month is 6 && (day is 5 or 6 or 21))
            return true;

        if (month is 7)
            return true;

        if (month is 11 && day is 1)
            return true;

        if (month is 12 && (day is 24 or 25 or 26 or 31))
            return true;

        return false;
    }
}