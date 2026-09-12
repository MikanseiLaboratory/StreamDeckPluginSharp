namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Zero-based slot coordinates on a Stream Deck device.
/// </summary>
public sealed class Coordinates
{
    public int Column { get; init; }

    public int Row { get; init; }
}
