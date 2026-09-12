using System.Text.Json;

namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Raw inbound Stream Deck message before action-specific dispatch.
/// </summary>
public sealed class IncomingMessage
{
    public string Event { get; init; } = "";

    public string? Action { get; init; }

    public string? Context { get; init; }

    public string? Device { get; init; }

    public string? Id { get; init; }

    public DeviceInfo? DeviceInfo { get; init; }

    public JsonElement Payload { get; init; }

    public bool HasPayload => Payload.ValueKind is not JsonValueKind.Undefined and not JsonValueKind.Null;
}
