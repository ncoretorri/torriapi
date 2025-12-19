using Torri.Services;

namespace Torri.HostedServices;

public class KodiCheker(ILogger<KodiCheker> logger, IServiceProvider serviceProvider) : IHostedService
{
    private Timer? _timer;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timer = new Timer(CallBack, null, TimeSpan.FromSeconds(10), Timeout.InfiniteTimeSpan);
        return Task.CompletedTask;
    }

    private async void CallBack(object? state)
    {
        logger.LogInformation("Check torrents fired");

        try
        {
            await using var scope = serviceProvider.CreateAsyncScope();
            var torrentService = scope.ServiceProvider.GetRequiredService<ITorrentService>();
            await torrentService.CheckAllAsync(CancellationToken.None);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to check");
        }

        _timer!.Change(TimeSpan.FromSeconds(20), Timeout.InfiniteTimeSpan);
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }
}