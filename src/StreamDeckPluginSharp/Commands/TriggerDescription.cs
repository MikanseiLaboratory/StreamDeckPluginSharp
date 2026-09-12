namespace StreamDeckPluginSharp.Commands;

/// <summary>
/// Encoder accessibility labels sent with <c>setTriggerDescription</c>.
/// </summary>
public sealed class TriggerDescription
{
    public string? Rotate { get; init; }

    public string? Push { get; init; }

    public string? Touch { get; init; }

    public string? LongTouch { get; init; }
}
