﻿namespace Norion.FeeHandling;

public interface IFeeHandler
{
    IFeeHandler SetNext(IFeeHandler next);
    int CalculateFee(DateTimeOffset time);
}
