using System.Text.Json;
using System.Text.RegularExpressions;
using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Display;

namespace Torri.Tests.TestLogger;

public class TestSink(LogEventLevel restrictedToMinimumLevel) : ILogEventSink
{
    private static readonly JsonSerializerOptions Options = new()
    {
        MaxDepth = 10
    };

    private static readonly MessageTemplateTextFormatter MessageTemplateTextFormatter = new("{Message:lj}");

    private static volatile bool _isEnabled = true;

    public static ITestOutputHelper Output { get; set; } = null!;

    public static bool IsEnabled
    {
        get => _isEnabled;
        set => _isEnabled = value;
    }

    public static bool LogProperties { get; set; } = false;

    public void Emit(LogEvent logEvent)
    {
        if (!_isEnabled || restrictedToMinimumLevel > logEvent.Level)
            return;

        var sw = new StringWriter();
        MessageTemplateTextFormatter.Format(logEvent, sw);
        var message = Regex.Unescape(sw.ToString());

        try
        {
            if (LogProperties)
            {
                var values = logEvent.Properties.ToDictionary(x => x.Key,
                    x => x.Value.ToString().Replace("\u0022", string.Empty));
                Output.WriteLine(
                    $"{logEvent.Level.ToString()[0]}: {message} | {JsonSerializer.Serialize(values, Options)}");
            }
            else
            {
                Output.WriteLine($"{logEvent.Level.ToString()[0]}: {message}");
            }

            if (logEvent.Exception != null)
                Output.WriteLine(logEvent.Exception.ToString());
            Output.WriteLine(string.Empty);
        }
        catch
        {
            // Nothing to do
        }
    }
}