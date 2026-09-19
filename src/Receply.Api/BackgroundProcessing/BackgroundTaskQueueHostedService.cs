namespace Receply.Api.BackgroundProcessing;

public class BackgroundTaskQueueHostedService(
    IBackgroundTaskQueue taskQueue,
    IServiceScopeFactory scopeFactory,
    ILogger<BackgroundTaskQueueHostedService> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            var workItem = await taskQueue.DequeueAsync(stoppingToken);

            using var scope = scopeFactory.CreateScope();
            try
            {
                await workItem(scope.ServiceProvider, stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError(ex, "Unhandled exception processing a background work item.");
            }
        }
    }
}
