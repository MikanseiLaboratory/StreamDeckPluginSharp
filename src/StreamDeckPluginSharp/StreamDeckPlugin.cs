using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamDeckPluginSharp.Connection;
using StreamDeckPluginSharp.Hosting;

namespace StreamDeckPluginSharp;

/// <summary>
/// Host that connects to Stream Deck, starts shared services, and dispatches action events.
/// </summary>
public sealed class StreamDeckPlugin : IAsyncDisposable
{
    private readonly ServiceProvider _services;
    private readonly StreamDeckConnection _connection;
    private readonly EventDispatcher _dispatcher;
    private readonly IReadOnlyList<Type> _pluginServiceTypes;

    internal StreamDeckPlugin(
        ServiceProvider services,
        StreamDeckConnection connection,
        EventDispatcher dispatcher,
        IReadOnlyList<Type> pluginServiceTypes)
    {
        _services = services;
        _connection = connection;
        _dispatcher = dispatcher;
        _pluginServiceTypes = pluginServiceTypes;
    }

    public IServiceProvider Services => _services;

    public IStreamDeckConnection Connection => _connection;

    public static StreamDeckPluginBuilder CreateBuilder(string[] args)
    {
        var arguments = RegistrationArguments.Parse(args);
        var builder = new StreamDeckPluginBuilder(arguments);
        var entry = Assembly.GetEntryAssembly();
        if (entry is not null)
        {
            builder.AddActionsFromAssembly(entry);
        }

        return builder;
    }

    public static Task RunAsync(string[] args, CancellationToken cancellationToken = default)
    {
        return CreateBuilder(args).Build().RunAsync(cancellationToken);
    }

    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        Console.CancelKeyPress += (_, e) =>
        {
            e.Cancel = true;
            linked.Cancel();
        };

        var pluginServices = _pluginServiceTypes
            .Select(type => _services.GetService(type))
            .OfType<IPluginService>()
            .Distinct()
            .ToArray();
        foreach (var service in pluginServices)
        {
            await service.StartAsync(linked.Token).ConfigureAwait(false);
        }

        try
        {
            await _connection.RunAsync(_dispatcher.DispatchAsync, linked.Token).ConfigureAwait(false);
        }
        finally
        {
            foreach (var service in pluginServices.Reverse())
            {
                try
                {
                    await service.StopAsync(CancellationToken.None).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _services.GetService<ILogger<StreamDeckPlugin>>()?.LogError(ex, "Failed to stop plugin service {Service}.", service.GetType().Name);
                }

                if (service is IAsyncDisposable asyncDisposable)
                {
                    await asyncDisposable.DisposeAsync().ConfigureAwait(false);
                }
                else if (service is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            await _dispatcher.DisposeAllAsync().ConfigureAwait(false);
        }
    }

    public async ValueTask DisposeAsync()
    {
        await _connection.DisposeAsync().ConfigureAwait(false);
        await _services.DisposeAsync().ConfigureAwait(false);
    }
}
