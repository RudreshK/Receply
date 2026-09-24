namespace Receply.Api.BackgroundProcessing;

/// <summary>
/// Lets request handlers (e.g. the WhatsApp webhook) hand off slow work - here, calling an LLM -
/// so the HTTP response returns immediately instead of blocking on it. In-process only: fine for a
/// single-instance deployment; once Receply.Api runs on multiple instances, replace this with a
/// durable queue (Redis Streams / a message bus) consumed by Receply.Workers instead.
/// </summary>
public interface IBackgroundTaskQueue
{
    void QueueWorkItem(Func<IServiceProvider, CancellationToken, Task> workItem);

    Task<Func<IServiceProvider, CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
}
