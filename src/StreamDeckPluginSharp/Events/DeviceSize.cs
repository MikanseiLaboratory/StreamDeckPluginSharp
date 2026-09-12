namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Key grid size reported for a connected device.
/// </summary>
public sealed class DeviceSize
{
    public int Columns { get; init; }

    public int Rows { get; init; }
}
