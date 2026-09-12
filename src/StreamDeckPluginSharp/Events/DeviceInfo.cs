namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Device metadata provided during registration and device events.
/// </summary>
public sealed class DeviceInfo
{
    public string Id { get; init; } = "";

    public string Name { get; init; } = "";

    public DeviceSize Size { get; init; } = new();

    public DeviceType Type { get; init; }
}
