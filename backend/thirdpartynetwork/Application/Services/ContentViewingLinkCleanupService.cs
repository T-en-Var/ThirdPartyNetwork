using thirdpartynetwork.Core.Interfaces.Repositories;

namespace thirdpartynetwork.Application.Services;

/// <summary>
/// PLAN.md §2.3: periodic sweep deleting expired ContentViewingLink rows — persisted in
/// Postgres, not an in-process store (DATABASE_AND_AUTH_OVERVIEW.md §6.4 already flags
/// AuthorizationCodeStore's in-memory-only design as a landmine for horizontal scaling;
/// this sweep itself is safe to run from every instance since the delete is idempotent).
/// </summary>
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
                // Never let a sweep failure take the whole app down — same "log and
                // continue" convention as the startup migrate/seed block in Program.cs.
                logger.LogError(ex, "ContentViewingLink cleanup sweep failed.");
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}
