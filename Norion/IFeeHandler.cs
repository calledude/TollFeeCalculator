namespace Norion;

public interface IFeeHandler
{
    IFeeHandler SetNext(IFeeHandler next);
    int CalculateFee(DateTime time);
}
