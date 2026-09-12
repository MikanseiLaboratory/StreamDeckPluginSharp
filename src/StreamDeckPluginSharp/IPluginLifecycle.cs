using System.Text.Json;
using StreamDeckPluginSharp.Events;

namespace StreamDeckPluginSharp;

/// <summary>
/// Optional plugin-wide event sink for device, application, deep-link, and secrets events.
/// </summary>
public interface IPluginLifecycle
{
    Task OnApplicationDidLaunchAsync(string application, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnApplicationDidTerminateAsync(string application, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnDeviceDidConnectAsync(string deviceId, DeviceInfo info, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnDeviceDidDisconnectAsync(string deviceId, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnDeviceDidChangeAsync(string deviceId, DeviceInfo info, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnSystemDidWakeUpAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnDidReceiveDeepLinkAsync(string url, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnDidReceiveGlobalSettingsAsync(JsonElement settings, CancellationToken cancellationToken) => Task.CompletedTask;

    Task OnDidReceiveSecretsAsync(JsonElement secrets, CancellationToken cancellationToken) => Task.CompletedTask;
}
