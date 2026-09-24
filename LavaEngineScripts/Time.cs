namespace LavaEngineScripts.Time;

using LavaEngine.TimeCore;
using LavaEngine.Log;

using TimeLevel = LavaEngine.TimeCore.Time.TimeLevel;

public class Time
{
    private Logger _l = new Logger();
    private LavaEngine.TimeCore.Time _t = new LavaEngine.TimeCore.Time();
    
    public int GetCurrentTimeOfSpecifiedLevel(string level)
    {
        switch (level)
        {
            case "Year":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.YEAR);
            case "Month":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.MONTH);
            case "Day":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.DAY);
            case "Hour":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.HOUR);
            case "Minute":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.MINUTE);
            case "Second":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.SECOND);
            case "Millisecond":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.MILLISECOND);
            case "Microsecond":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.MICROSECOND);
            case "Nanosecond":
                return _t.getCurrentTimeOfSpecifiedLevel(TimeLevel.NANOSECOND);
            default:
                _l.Error($"invalid log level: {level}");
                return -1;
        }
    }
}