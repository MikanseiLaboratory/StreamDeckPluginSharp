using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StreamDeckPluginSharp.Connection;
using StreamDeckPluginSharp.Hosting;

namespace StreamDeckPluginSharp;

/// <summary>
/// Configures action discovery, shared services, and the Stream Deck connection.
/// </summary>
public sealed class StreamDeckPluginBuilder
{
    private readonly RegistrationArguments _arguments;
    private readonly ActionRegistry _registry = new();
    private IStreamDeckTransport? _transport;

    internal StreamDeckPluginBuilder(RegistrationArguments arguments)
    {
        _arguments = arguments;
        Services = new ServiceCollection();
    }

    public IServiceCollection Services { get; }

    public RegistrationArguments Arguments => _arguments;

    public StreamDeckPluginBuilder AddActionsFromAssembly(Assembly assembly)
    {
        _registry.AddFromAssembly(assembly);
        return this;
    }

    public StreamDeckPluginBuilder AddAction<TAction>(string uuid) where TAction : Actions.ActionBase
    {
        _registry.Add(uuid, typeof(TAction));
        return this;
    }

    internal StreamDeckPluginBuilder UseTransport(IStreamDeckTransport transport)
    {
        _transport = transport;
        return this;
    }

    public StreamDeckPlugin Build()
    {
        var connection = new StreamDeckConnection(
            _arguments,
            _transport,
            NullLogger<StreamDeckConnection>.Instance);
        Services.AddSingleton<IStreamDeckConnection>(connection);
        Services.AddSingleton(_registry);

        var pluginServiceTypes = Services
            .Select(descriptor => descriptor.ImplementationType ?? descriptor.ServiceType)
            .Where(type => type is { IsAbstract: false, IsInterface: false } && typeof(IPluginService).IsAssignableFrom(type))
            .Distinct()
            .ToArray();

        var provider = Services.BuildServiceProvider();
        var logger = provider.GetService<ILogger<EventDispatcher>>() ?? NullLogger<EventDispatcher>.Instance;
        var dispatcher = new EventDispatcher(provider, _registry, connection, logger);
        return new StreamDeckPlugin(provider, connection, dispatcher, pluginServiceTypes);
    }
}
