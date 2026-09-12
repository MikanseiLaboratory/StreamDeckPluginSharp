using StreamDeckPluginSharp;

namespace CounterSample.Services;

public sealed class CounterStore : IPluginService
{
    private int _count;

    public int Count => Volatile.Read(ref _count);

    public event Action<int>? Changed;

    public int Add(int delta)
    {
        var next = Interlocked.Add(ref _count, delta);
        Changed?.Invoke(next);
        return next;
    }

    public Task StartAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
