using System;
public struct RoundedTimeSpan
{

    private const int TIMESPAN_SIZE = 7; // it always has seven digits

    private TimeSpan roundedTimeSpan;
    private int precision;

    public RoundedTimeSpan(long ticks, int precision)
    {
        if (precision < 0) { throw new ArgumentException("precision must be non-negative"); }
        this.precision = precision;
        int factor = (int)System.Math.Pow(10, (TIMESPAN_SIZE - precision));

        // This is only valid for rounding milliseconds-will *not* work on secs/mins/hrs!
        roundedTimeSpan = new TimeSpan(((long)System.Math.Round((1.0 * ticks / factor)) * factor));
    }

    public TimeSpan TimeSpan { get { return roundedTimeSpan; } }
}

