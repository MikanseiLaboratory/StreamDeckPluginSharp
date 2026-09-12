using System.Text.Json;
using StreamDeckPluginSharp.Commands;
using StreamDeckPluginSharp.Events;
using StreamDeckPluginSharp.Serialization;

namespace StreamDeckPluginSharp.Actions;

/// <summary>
/// Untyped action instance bound to a single Stream Deck context.
/// </summary>
public abstract class ActionBase : IAsyncDisposable
{
    public IStreamDeckConnection Connection { get; private set; } = null!;

    public string Context { get; private set; } = "";

    public string ActionId { get; private set; } = "";

    public string DeviceId { get; private set; } = "";

    public Coordinates? Coordinates { get; private set; }

    public Controller Controller { get; private set; }

    public bool IsInMultiAction { get; private set; }

    internal void Attach(IStreamDeckConnection connection, IncomingMessage message, ActionPayload payload)
    {
        Connection = connection;
        Context = message.Context ?? "";
        ActionId = message.Action ?? "";
        DeviceId = message.Device ?? "";
        Coordinates = payload.Coordinates;
        Controller = payload.Controller;
        IsInMultiAction = payload.IsInMultiAction;
        ApplySettings(payload.Settings);
    }

    internal virtual void ApplySettings(JsonElement settings)
    {
    }

    internal virtual object? CurrentSettings() => null;

    internal virtual Task NotifySettingsChangedAsync(object? previous, CancellationToken cancellationToken)
        => OnSettingsChangedAsync(cancellationToken);

    public virtual Task OnWillAppearAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnWillDisappearAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnDidReceiveSettingsAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnSettingsChangedAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnPropertyInspectorDidAppearAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnPropertyInspectorDidDisappearAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnPropertyInspectorMessageAsync(JsonElement payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnTitleParametersDidChangeAsync(TitleParametersPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnDidReceiveResourcesAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnKeyDownAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnKeyUpAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnDialDownAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnDialUpAsync(ActionPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnDialRotateAsync(DialRotatePayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public virtual Task OnTouchTapAsync(TouchTapPayload payload, CancellationToken cancellationToken) => Task.CompletedTask;

    public Task SetTitleAsync(string? title, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Connection.SetTitleAsync(Context, title, target, state, cancellationToken);

    public Task SetImageAsync(string? image, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Connection.SetImageAsync(Context, image, target, state, cancellationToken);

    public Task SetImageAsync(byte[] pngBytes, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Connection.SetImageAsync(Context, pngBytes, target, state, cancellationToken);

    public Task SetImageFromFileAsync(string path, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => Connection.SetImageFromFileAsync(Context, path, target, state, cancellationToken);

    public Task SetStateAsync(int state, CancellationToken cancellationToken = default)
        => Connection.SetStateAsync(Context, state, cancellationToken);

    public Task SetSettingsAsync(object settings, CancellationToken cancellationToken = default)
        => Connection.SetSettingsAsync(Context, settings, cancellationToken);

    public Task GetSettingsAsync(string? id = null, CancellationToken cancellationToken = default)
        => Connection.GetSettingsAsync(Context, id, cancellationToken);

    public Task ShowOkAsync(CancellationToken cancellationToken = default)
        => Connection.ShowOkAsync(Context, cancellationToken);

    public Task ShowAlertAsync(CancellationToken cancellationToken = default)
        => Connection.ShowAlertAsync(Context, cancellationToken);

    public Task SendToPropertyInspectorAsync<T>(T payload, CancellationToken cancellationToken = default)
        => Connection.SendToPropertyInspectorAsync(Context, payload!, cancellationToken);

    public Task SetFeedbackAsync(object payload, CancellationToken cancellationToken = default)
        => Connection.SetFeedbackAsync(Context, payload, cancellationToken);

    public Task SetFeedbackLayoutAsync(string layout, CancellationToken cancellationToken = default)
        => Connection.SetFeedbackLayoutAsync(Context, layout, cancellationToken);

    public Task SetTriggerDescriptionAsync(TriggerDescription description, CancellationToken cancellationToken = default)
        => Connection.SetTriggerDescriptionAsync(Context, description, cancellationToken);

    public Task GetResourcesAsync(string? id = null, CancellationToken cancellationToken = default)
        => Connection.GetResourcesAsync(Context, id, cancellationToken);

    public Task SetResourcesAsync(IReadOnlyDictionary<string, string> resources, CancellationToken cancellationToken = default)
        => Connection.SetResourcesAsync(Context, resources, cancellationToken);

    public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;
}

/// <summary>
/// Action instance with automatically synchronized typed settings.
/// </summary>
public abstract class ActionBase<TSettings> : ActionBase where TSettings : class, new()
{
    public TSettings Settings { get; private set; } = new();

    internal override void ApplySettings(JsonElement settings)
    {
        Settings = settings.ValueKind == JsonValueKind.Object
            ? StreamDeckJson.Deserialize<TSettings>(settings) ?? new TSettings()
            : new TSettings();
    }

    internal override object? CurrentSettings() => Settings;

    public Task SetSettingsAsync(CancellationToken cancellationToken = default)
        => SetSettingsAsync(Settings, cancellationToken);

    public async Task UpdateSettingsAsync(Action<TSettings> mutate, CancellationToken cancellationToken = default)
    {
        mutate(Settings);
        await SetSettingsAsync(cancellationToken).ConfigureAwait(false);
    }

    public virtual Task OnSettingsChangedAsync(TSettings previous, TSettings current, CancellationToken cancellationToken)
        => Task.CompletedTask;

    internal override async Task NotifySettingsChangedAsync(object? previous, CancellationToken cancellationToken)
    {
        var previousSettings = previous as TSettings ?? new TSettings();
        await OnSettingsChangedAsync(previousSettings, Settings, cancellationToken).ConfigureAwait(false);
        await OnSettingsChangedAsync(cancellationToken).ConfigureAwait(false);
    }
}

public abstract class KeyActionBase<TSettings> : ActionBase<TSettings> where TSettings : class, new();

public abstract class EncoderActionBase<TSettings> : ActionBase<TSettings> where TSettings : class, new();

public abstract class KeyAndEncoderActionBase<TSettings> : ActionBase<TSettings> where TSettings : class, new();
