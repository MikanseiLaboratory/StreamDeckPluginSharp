using System.Text.Json;
using CounterSample.Contracts;
using CounterSample.Services;
using StreamDeckPluginSharp;
using StreamDeckPluginSharp.Actions;
using StreamDeckPluginSharp.Commands;
using StreamDeckPluginSharp.Events;

namespace CounterSample.Actions;

[StreamDeckAction("dev.flowingspdg.countersample.dial")]
public sealed class DialAction(CounterStore store) : EncoderActionBase<CounterSettings>
{
    public override async Task OnWillAppearAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        store.Changed += OnStoreChanged;
        await SetTriggerDescriptionAsync(new TriggerDescription
        {
            Rotate = "Change shared count",
            Push = "Reset shared count"
        }, cancellationToken).ConfigureAwait(false);
        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task OnWillDisappearAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        store.Changed -= OnStoreChanged;
        return Task.CompletedTask;
    }

    public override async Task OnDialRotateAsync(DialRotatePayload payload, CancellationToken cancellationToken)
    {
        store.Add(payload.Ticks * Math.Max(1, Settings.Increment));
        await Task.CompletedTask;
    }

    public override async Task OnDialDownAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        store.Add(-store.Count);
        await ShowOkAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task OnSettingsChangedAsync(CounterSettings previous, CounterSettings current, CancellationToken cancellationToken)
        => RefreshAsync(cancellationToken);

    public override Task OnPropertyInspectorDidAppearAsync(CancellationToken cancellationToken)
        => SendCountAsync(cancellationToken);

    public override Task OnPropertyInspectorMessageAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        InspectorBridge.Apply(store, InspectorBridge.ReadCommandType(payload), Math.Max(1, Settings.Increment));
        return SendCountAsync(cancellationToken);
    }

    private void OnStoreChanged(int count)
    {
        _ = RefreshAsync(CancellationToken.None);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        var label = string.IsNullOrWhiteSpace(Settings.Label) ? "Count" : Settings.Label;
        await SetTitleAsync($"{label}: {store.Count}", cancellationToken: cancellationToken).ConfigureAwait(false);
        await SetFeedbackAsync(new Dictionary<string, object>
        {
            ["title"] = label,
            ["value"] = store.Count.ToString(),
            ["indicator"] = Math.Clamp(store.Count, 0, 100)
        }, cancellationToken).ConfigureAwait(false);
        await SendCountAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task SendCountAsync(CancellationToken cancellationToken)
        => SendToPropertyInspectorAsync(new CountChangedMessage { Count = store.Count }, cancellationToken);
}
