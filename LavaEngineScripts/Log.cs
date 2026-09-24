namespace LavaEngineScripts.Log;

using LavaEngine.Log;

public class Logger
{ 
    LavaEngine.Log.Logger _logger = new LavaEngine.Log.Logger();

    public void SetMinLogLevel(int level)
    {
        if (level > Enum.GetValues(typeof(LavaEngine.Log.Logger.LogLevel)).Length)
        {
            _logger.Error("invalid log level.");
        }

        var engineLevel = (LavaEngine.Log.Logger.LogLevel)Enum.ToObject(typeof(LavaEngine.Log.Logger.LogLevel), level);
        
        _logger.SetMinLogLevel(engineLevel);
    } 

    public void SetPattern(string pattern) => _logger.SetPattern(pattern);
    public void Trace(String message) => _logger.LogMes(message, LavaEngine.Log.Logger.LogLevel.Trace);
    public void Debug(String message) => _logger.LogMes(message, LavaEngine.Log.Logger.LogLevel.Debug);
    public void Info(String message) => _logger.LogMes(message, LavaEngine.Log.Logger.LogLevel.Info);
    public void Warning(String message) => _logger.LogMes(message, LavaEngine.Log.Logger.LogLevel.Warning);
    public void Error(String message) => _logger.LogMes(message, LavaEngine.Log.Logger.LogLevel.Error);
    public void Critical(String message) => _logger.LogMes(message, LavaEngine.Log.Logger.LogLevel.Critical);
}