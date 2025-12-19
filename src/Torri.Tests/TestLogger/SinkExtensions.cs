using Serilog;
using Serilog.Configuration;
using Serilog.Events;

namespace Torri.Tests.TestLogger;

public static class SinkExtensions
{
    public static LoggerConfiguration TestSink(
        this LoggerSinkConfiguration loggerConfiguration,
        IFormatProvider? formatProvider = null,
        LogEventLevel restrictedToMinimumLevel = LogEventLevel.Debug)
    {
        return loggerConfiguration.Sink(new TestSink(restrictedToMinimumLevel));
    }
}