using System.Text.Json;

namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Common payload fields shared by action-scoped Stream Deck events.
/// </summary>
public class ActionPayload
{
    public Controller Controller { get; init; } = Controller.Keypad;

    public Coordinates? Coordinates { get; init; }

    public bool IsInMultiAction { get; init; }

    public int? State { get; init; }

    public int? UserDesiredState { get; init; }

    public IReadOnlyDictionary<string, string> Resources { get; init; } = new Dictionary<string, string>();

    public JsonElement Settings { get; init; }
}

public sealed class DialRotatePayload : ActionPayload
{
    public bool Pressed { get; set; }

    public int Ticks { get; set; }
}

public sealed class TouchTapPayload : ActionPayload
{
    public bool Hold { get; set; }

    public int TapX { get; set; }

    public int TapY { get; set; }
}

public sealed class TitleParametersPayload : ActionPayload
{
    public string Title { get; set; } = "";

    public TitleParameters TitleParameters { get; set; } = new();
}

public sealed class TitleParameters
{
    public string FontFamily { get; init; } = "";

    public double FontSize { get; init; }

    public string FontStyle { get; init; } = "";

    public bool FontUnderline { get; init; }

    public bool ShowTitle { get; init; }

    public string TitleAlignment { get; init; } = "";

    public string TitleColor { get; init; } = "";
}

public sealed class ApplicationEventPayload
{
    public string Application { get; init; } = "";
}

public sealed class DeepLinkPayload
{
    public string Url { get; init; } = "";
}

public sealed class SettingsEnvelope
{
    public JsonElement Settings { get; init; }
}

public sealed class SecretsEnvelope
{
    public JsonElement Secrets { get; init; }
}
