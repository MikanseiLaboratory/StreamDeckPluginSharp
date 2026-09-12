using StreamDeckPluginSharp.Commands;
using StreamDeckPluginSharp.Events;

namespace StreamDeckPluginSharp.Tests.Fakes;

internal sealed class FakeStreamDeckConnection : IStreamDeckConnection
{
    public List<(string Event, string? Context, object? Payload)> Sent { get; } = [];

    public string PluginUuid { get; } = "dev.flowingspdg.test";

    public RegistrationInfo Info { get; } = new();

    public Task SetTitleAsync(string context, string? title, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Record("setTitle", context, new { title, target, state });

    public Task SetImageAsync(string context, string? image, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Record("setImage", context, image);

    public Task SetImageAsync(string context, byte[] pngBytes, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Record("setImage", context, pngBytes.Length);

    public Task SetImageFromFileAsync(string context, string path, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Record("setImage", context, path);

    public Task SetStateAsync(string context, int state, CancellationToken cancellationToken = default)
        => Record("setState", context, state);

    public Task SetSettingsAsync(string context, object settings, CancellationToken cancellationToken = default)
        => Record("setSettings", context, settings);

    public Task GetSettingsAsync(string context, string? id = null, CancellationToken cancellationToken = default)
        => Record("getSettings", context, id);

    public Task SetGlobalSettingsAsync(object settings, CancellationToken cancellationToken = default)
        => Record("setGlobalSettings", null, settings);

    public Task GetGlobalSettingsAsync(string? id = null, CancellationToken cancellationToken = default)
        => Record("getGlobalSettings", null, id);

    public Task ShowOkAsync(string context, CancellationToken cancellationToken = default)
        => Record("showOk", context, null);

    public Task ShowAlertAsync(string context, CancellationToken cancellationToken = default)
        => Record("showAlert", context, null);

    public Task SendToPropertyInspectorAsync(string context, object payload, CancellationToken cancellationToken = default)
        => Record("sendToPropertyInspector", context, payload);

    public Task SetFeedbackAsync(string context, object payload, CancellationToken cancellationToken = default)
        => Record("setFeedback", context, payload);

    public Task SetFeedbackLayoutAsync(string context, string layout, CancellationToken cancellationToken = default)
        => Record("setFeedbackLayout", context, layout);

    public Task SetTriggerDescriptionAsync(string context, TriggerDescription description, CancellationToken cancellationToken = default)
        => Record("setTriggerDescription", context, description);

    public Task OpenUrlAsync(string url, CancellationToken cancellationToken = default)
        => Record("openUrl", null, url);

    public Task LogMessageAsync(string message, CancellationToken cancellationToken = default)
        => Record("logMessage", null, message);

    public Task SwitchToProfileAsync(string device, string? profile = null, int? page = null, CancellationToken cancellationToken = default)
        => Record("switchToProfile", device, new { profile, page });

    public Task GetSecretsAsync(CancellationToken cancellationToken = default)
        => Record("getSecrets", null, null);

    public Task GetResourcesAsync(string context, string? id = null, CancellationToken cancellationToken = default)
        => Record("getResources", context, id);

    public Task SetResourcesAsync(string context, IReadOnlyDictionary<string, string> resources, CancellationToken cancellationToken = default)
        => Record("setResources", context, resources);

    private Task Record(string eventName, string? context, object? payload)
    {
        Sent.Add((eventName, context, payload));
        return Task.CompletedTask;
    }
}
