using System.Text.Json;
using StreamDeckPluginSharp.Actions;
using StreamDeckPluginSharp.Events;

namespace StreamDeckPluginSharp.Tests.Fakes;

[StreamDeckAction("dev.test.counter")]
public sealed class RecordingAction : KeyActionBase<RecordingSettings>
{
    public List<string> Events { get; } = [];

    public override Task OnWillAppearAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        Events.Add("willAppear");
        return Task.CompletedTask;
    }

    public override Task OnKeyDownAsync(ActionPayload payload, CancellationToken cancellationToken)
    {
        Events.Add($"keyDown:{Settings.Increment}");
        return Task.CompletedTask;
    }

    public override Task OnSettingsChangedAsync(RecordingSettings previous, RecordingSettings current, CancellationToken cancellationToken)
    {
        Events.Add($"settings:{previous.Increment}->{current.Increment}");
        return Task.CompletedTask;
    }

    public override Task OnPropertyInspectorMessageAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        Events.Add("pi");
        return Task.CompletedTask;
    }

    public override Task OnDialRotateAsync(DialRotatePayload payload, CancellationToken cancellationToken)
    {
        Events.Add($"dial:{payload.Ticks}");
        return Task.CompletedTask;
    }
}

[TypeScriptContract]
public sealed class RecordingSettings
{
    public int Increment { get; set; } = 1;
}

public sealed class SharedProbe : IPluginService
{
    public bool Started { get; private set; }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        Started = true;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        Started = false;
        return Task.CompletedTask;
    }
}
