using Norion.Vehicles;
using TollFeeCalculator;

namespace Norion;

public static class Program
{
    public static void Main()
    {
        var offset = DateTimeOffset.Now.Offset;
        var date = new DateTimeOffset(2024, 12, 17, 08, 00, 00, offset);

        var fee = TollCalculator.GetTollFee(new Car(), [date]);
        Console.WriteLine(fee);
    }
}
