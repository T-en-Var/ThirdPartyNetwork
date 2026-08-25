using thirdpartynetwork.Core.Interfaces.Repositories;

namespace thirdpartynetwork.Application.Services;

public class ContentViewingLinkCleanupService(IServiceScopeFactory scopeFactory, ILogger<ContentViewingLinkCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(SweepInterval);

        do
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IContentViewingLinkRepository>();
                var deleted = await repository.DeleteExpiredAsync(DateTime.UtcNow);
                if (deleted > 0)
                {
                    logger.LogInformation("ContentViewingLink cleanup: deleted {Count} expired link(s).", deleted);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "ContentViewingLink cleanup sweep failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
