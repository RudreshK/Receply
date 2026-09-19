using System.Threading.Channels;

namespace Receply.Api.BackgroundProcessing;

public class BackgroundTaskQueue : IBackgroundTaskQueue
{
    private readonly Channel<Func<IServiceProvider, CancellationToken, Task>> _channel =
        Channel.CreateUnbounded<Func<IServiceProvider, CancellationToken, Task>>(
            new UnboundedChannelOptions { SingleReader = true });

    public void QueueWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem) =>
        _channel.Writer.TryWrite(workItem);

    public async Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken) =>
        await _channel.Reader.ReadAsync(cancellationToken);
}
