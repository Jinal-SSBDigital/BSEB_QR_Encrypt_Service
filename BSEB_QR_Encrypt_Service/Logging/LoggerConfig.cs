using Serilog;

public static class LoggerConfig
{
    public static void Configure()
    {
        Log.Logger = new LoggerConfiguration().WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day).CreateLogger();
    }
}