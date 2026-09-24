namespace LavaEngine.TimeCore;

public class Time
{
    public enum TimeLevel { YEAR, MONTH, DAY, HOUR, MINUTE, SECOND, MILLISECOND, MICROSECOND, NANOSECOND };

    public int getCurrentTimeOfSpecifiedLevel(TimeLevel level)
    {
        var now = DateTime.Now;

        switch (level)
        {
            case TimeLevel.YEAR:
                return now.Year;
            case TimeLevel.MONTH:
                return now.Month;
            case TimeLevel.DAY:
                return now.Day;
            case TimeLevel.HOUR:
                return now.Hour;
            case TimeLevel.MINUTE:
                return now.Minute;
            case TimeLevel.SECOND:
                return now.Second;
            case TimeLevel.MILLISECOND:
                return now.Millisecond;
            case TimeLevel.MICROSECOND:
                return (int)((DateTime.Now.Ticks % TimeSpan.TicksPerSecond) / 10 % 1000000);
            case TimeLevel.NANOSECOND:
                return (int)((DateTime.Now.Ticks % TimeSpan.TicksPerSecond) % 100);
            default:
                return -1;
        }
    }
}
