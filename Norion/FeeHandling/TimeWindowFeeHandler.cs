namespace Norion.FeeHandling;

public class TimeWindowFeeHandler : IFeeHandler
{
    private readonly int _fee;
    private readonly TimeOnly _start;
    private readonly TimeOnly _end;
    private IFeeHandler? _next;

    public TimeWindowFeeHandler(TimeOnly start, TimeOnly end, int fee)
    {
        _start = start;
        _end = end;
        _fee = fee;
    }

    public int CalculateFee(DateTimeOffset time)
    {
        var timeOnly = TimeOnly.FromDateTime(time.Date);
        if (_start >= timeOnly && _end <= timeOnly)
            return _fee;

        return _next?.CalculateFee(time) ?? 0;
    }

    public IFeeHandler SetNext(IFeeHandler next)
    {
        return _next = next;
    }
}
