using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamDeckPluginSharp.Actions;
using StreamDeckPluginSharp.Events;
using StreamDeckPluginSharp.Serialization;

namespace StreamDeckPluginSharp.Hosting;

internal sealed class EventDispatcher
{
    private readonly IServiceProvider _services;
    private readonly ActionRegistry _registry;
    private readonly IStreamDeckConnection _connection;
    private readonly ILogger<EventDispatcher> _logger;
    private readonly ConcurrentDictionary<string, ActionBase> _instances = new(StringComparer.Ordinal);

    public EventDispatcher(
        IServiceProvider services,
        ActionRegistry registry,
        IStreamDeckConnection connection,
        ILogger<EventDispatcher> logger)
    {
        _services = services;
        _registry = registry;
        _connection = connection;
        _logger = logger;
    }

    public async Task DispatchAsync(IncomingMessage message, CancellationToken cancellationToken)
    {
        switch (message.Event)
        {
            case EventNames.WillAppear:
                await AppearAsync(message, cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.WillDisappear:
                await DisappearAsync(message, cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.ApplicationDidLaunch:
                await ForEachLifecycleAsync((hook, ct) => hook.OnApplicationDidLaunchAsync(ReadApplication(message), ct), cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.ApplicationDidTerminate:
                await ForEachLifecycleAsync((hook, ct) => hook.OnApplicationDidTerminateAsync(ReadApplication(message), ct), cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.DeviceDidConnect:
                if (message.Device is { } connected && message.DeviceInfo is { } connectedInfo)
                {
                    await ForEachLifecycleAsync((hook, ct) => hook.OnDeviceDidConnectAsync(connected, connectedInfo, ct), cancellationToken).ConfigureAwait(false);
                }
                return;
            case EventNames.DeviceDidDisconnect:
                if (message.Device is { } disconnected)
                {
                    await ForEachLifecycleAsync((hook, ct) => hook.OnDeviceDidDisconnectAsync(disconnected, ct), cancellationToken).ConfigureAwait(false);
                }
                return;
            case EventNames.DeviceDidChange:
                if (message.Device is { } changed && message.DeviceInfo is { } changedInfo)
                {
                    await ForEachLifecycleAsync((hook, ct) => hook.OnDeviceDidChangeAsync(changed, changedInfo, ct), cancellationToken).ConfigureAwait(false);
                }
                return;
            case EventNames.SystemDidWakeUp:
                await ForEachLifecycleAsync((hook, ct) => hook.OnSystemDidWakeUpAsync(ct), cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.DidReceiveDeepLink:
                await ForEachLifecycleAsync((hook, ct) => hook.OnDidReceiveDeepLinkAsync(ReadDeepLink(message), ct), cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.DidReceiveGlobalSettings:
                await ForEachLifecycleAsync((hook, ct) => hook.OnDidReceiveGlobalSettingsAsync(ReadNested(message, "settings"), ct), cancellationToken).ConfigureAwait(false);
                return;
            case EventNames.DidReceiveSecrets:
                await ForEachLifecycleAsync((hook, ct) => hook.OnDidReceiveSecretsAsync(ReadNested(message, "secrets"), ct), cancellationToken).ConfigureAwait(false);
                return;
        }

        if (message.Context is null || !_instances.TryGetValue(message.Context, out var action))
        {
            _logger.LogDebug("No action instance for {Event} / {Context}.", message.Event, message.Context);
            return;
        }

        await DispatchToActionAsync(action, message, cancellationToken).ConfigureAwait(false);
    }

    public async Task DisposeAllAsync()
    {
        foreach (var action in _instances.Values)
        {
            await action.DisposeAsync().ConfigureAwait(false);
        }

        _instances.Clear();
    }

    private async Task AppearAsync(IncomingMessage message, CancellationToken cancellationToken)
    {
        if (message.Context is null || message.Action is null)
        {
            return;
        }

        if (!_registry.TryGet(message.Action, out var actionType))
        {
            _logger.LogWarning("No action registered for UUID {Action}.", message.Action);
            return;
        }

        var payload = message.HasPayload ? ActionPayloadParser.Parse(message.Payload) : new ActionPayload();
        var action = (ActionBase)ActivatorUtilities.CreateInstance(_services, actionType);
        action.Attach(_connection, message, payload);
        _instances[message.Context] = action;
        await action.OnWillAppearAsync(payload, cancellationToken).ConfigureAwait(false);
    }

    private async Task DisappearAsync(IncomingMessage message, CancellationToken cancellationToken)
    {
        if (message.Context is null || !_instances.TryRemove(message.Context, out var action))
        {
            return;
        }

        var payload = message.HasPayload ? ActionPayloadParser.Parse(message.Payload) : new ActionPayload();
        try
        {
            await action.OnWillDisappearAsync(payload, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            await action.DisposeAsync().ConfigureAwait(false);
        }
    }

    private static async Task DispatchToActionAsync(ActionBase action, IncomingMessage message, CancellationToken cancellationToken)
    {
        switch (message.Event)
        {
            case EventNames.KeyDown:
                await action.OnKeyDownAsync(Parse(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.KeyUp:
                await action.OnKeyUpAsync(Parse(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.DialDown:
                await action.OnDialDownAsync(Parse(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.DialUp:
                await action.OnDialUpAsync(Parse(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.DialRotate:
                await action.OnDialRotateAsync(Parse<DialRotatePayload>(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.TouchTap:
                await action.OnTouchTapAsync(Parse<TouchTapPayload>(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.DidReceiveSettings:
                await ApplySettingsAsync(action, message, cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.SendToPlugin:
                await action.OnPropertyInspectorMessageAsync(message.Payload, cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.PropertyInspectorDidAppear:
                await action.OnPropertyInspectorDidAppearAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.PropertyInspectorDidDisappear:
                await action.OnPropertyInspectorDidDisappearAsync(cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.TitleParametersDidChange:
                await action.OnTitleParametersDidChangeAsync(Parse<TitleParametersPayload>(message), cancellationToken).ConfigureAwait(false);
                break;
            case EventNames.DidReceiveResources:
                await action.OnDidReceiveResourcesAsync(Parse(message), cancellationToken).ConfigureAwait(false);
                break;
        }
    }

    private static async Task ApplySettingsAsync(ActionBase action, IncomingMessage message, CancellationToken cancellationToken)
    {
        var payload = Parse(message);
        var previous = action.CurrentSettings();
        action.ApplySettings(payload.Settings);
        await action.OnDidReceiveSettingsAsync(payload, cancellationToken).ConfigureAwait(false);
        await action.NotifySettingsChangedAsync(previous, cancellationToken).ConfigureAwait(false);
    }

    private async Task ForEachLifecycleAsync(Func<IPluginLifecycle, CancellationToken, Task> invoke, CancellationToken cancellationToken)
    {
        foreach (var hook in _services.GetServices<IPluginLifecycle>())
        {
            await invoke(hook, cancellationToken).ConfigureAwait(false);
        }
    }

    private static ActionPayload Parse(IncomingMessage message)
        => message.HasPayload ? ActionPayloadParser.Parse(message.Payload) : new ActionPayload();

    private static T Parse<T>(IncomingMessage message) where T : ActionPayload, new()
        => message.HasPayload ? ActionPayloadParser.Parse<T>(message.Payload) : new T();

    private static string ReadApplication(IncomingMessage message)
        => message.HasPayload && message.Payload.TryGetProperty("application", out var value)
            ? value.GetString() ?? ""
            : "";

    private static string ReadDeepLink(IncomingMessage message)
        => message.HasPayload && message.Payload.TryGetProperty("url", out var value)
            ? value.GetString() ?? ""
            : "";

    private static JsonElement ReadNested(IncomingMessage message, string name)
        => message.HasPayload && message.Payload.TryGetProperty(name, out var value)
            ? value
            : default;
}
