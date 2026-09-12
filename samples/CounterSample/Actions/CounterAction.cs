using System.Text.Json;
using CounterSample.Contracts;
using CounterSample.Services;
using StreamDeckPluginSharp;
using StreamDeckPluginSharp.Actions;
using StreamDeckPluginSharp.Events;

namespace CounterSample.Actions;

[StreamDeckAction("dev.flowingspdg.countersample.counter")]
public sealed class CounterAction(CounterStore store) : KeyActionBase<CounterSettings>
{
    public override async Task OnWillAppearAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        store.Changed += OnStoreChanged;
        await RefreshAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task OnWillDisappearAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        store.Changed -= OnStoreChanged;
        return Task.CompletedTask;
    }

    public override async Task OnKeyDownAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        store.Add(Settings.Increment);
        await ShowOkAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task OnSettingsChangedAsync(CounterSettings previous, CounterSettings current, CancellationToken cancellationToken)
        => RefreshAsync(cancellationToken);

    public override async Task OnPropertyInspectorDidAppearAsync(CancellationToken cancellationToken)
    {
        await SendCountAsync(cancellationToken).ConfigureAwait(false);
    }

    public override Task OnPropertyInspectorMessageAsync(JsonElement payload, CancellationToken cancellationToken)
        => SendCountAsync(cancellationToken);

    private void OnStoreChanged(int count)
    {
        _ = RefreshAsync(CancellationToken.None);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken)
    {
        await SetTitleAsync($"{store.Count}\n+{Settings.Increment}", cancellationToken: cancellationToken).ConfigureAwait(false);
        await SendCountAsync(cancellationToken).ConfigureAwait(false);
    }

    private Task SendCountAsync(CancellationToken cancellationToken)
        => SendToPropertyInspectorAsync(new CountChangedMessage { Count = store.Count }, cancellationToken);
}
