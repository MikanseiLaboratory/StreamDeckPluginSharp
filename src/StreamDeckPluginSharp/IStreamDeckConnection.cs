using StreamDeckPluginSharp.Commands;
using StreamDeckPluginSharp.Events;

namespace StreamDeckPluginSharp;

/// <summary>
/// Plugin-wide Stream Deck command channel. Register as a singleton to send commands from shared services.
/// </summary>
public interface IStreamDeckConnection
{
    string PluginUuid { get; }

    RegistrationInfo Info { get; }

    Task SetTitleAsync(string context, string? title, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default);

    Task SetImageAsync(string context, string? image, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default);

    Task SetImageAsync(string context, byte[] pngBytes, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default);

    Task SetImageFromFileAsync(string context, string path, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default);

    Task SetStateAsync(string context, int state, CancellationToken cancellationToken = default);

    Task SetSettingsAsync(string context, object settings, CancellationToken cancellationToken = default);

    Task GetSettingsAsync(string context, string? id = null, CancellationToken cancellationToken = default);

    Task SetGlobalSettingsAsync(object settings, CancellationToken cancellationToken = default);

    Task GetGlobalSettingsAsync(string? id = null, CancellationToken cancellationToken = default);

    Task ShowOkAsync(string context, CancellationToken cancellationToken = default);

    Task ShowAlertAsync(string context, CancellationToken cancellationToken = default);

    Task SendToPropertyInspectorAsync(string context, object payload, CancellationToken cancellationToken = default);

    Task SetFeedbackAsync(string context, object payload, CancellationToken cancellationToken = default);

    Task SetFeedbackLayoutAsync(string context, string layout, CancellationToken cancellationToken = default);

    Task SetTriggerDescriptionAsync(string context, TriggerDescription description, CancellationToken cancellationToken = default);

    Task OpenUrlAsync(string url, CancellationToken cancellationToken = default);

    Task LogMessageAsync(string message, CancellationToken cancellationToken = default);

    Task SwitchToProfileAsync(string device, string? profile = null, int? page = null, CancellationToken cancellationToken = default);

    Task GetSecretsAsync(CancellationToken cancellationToken = default);

    Task GetResourcesAsync(string context, string? id = null, CancellationToken cancellationToken = default);

    Task SetResourcesAsync(string context, IReadOnlyDictionary<string, string> resources, CancellationToken cancellationToken = default);
}
