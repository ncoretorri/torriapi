namespace Torri.Helpers
{
    public static class LoggerExtensions
    {
        public static IDisposable StartScope(this ILogger logger, string scopeName, object value)
        {
            return logger.BeginScope(new Dictionary<string, object>
            {
                { scopeName, value }
            })!;
        }
    }
}
