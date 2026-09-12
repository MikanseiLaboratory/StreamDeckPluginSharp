namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Startup information supplied by Stream Deck through the <c>-info</c> argument.
/// </summary>
public sealed class RegistrationInfo
{
    public ApplicationInfo Application { get; init; } = new();

    public ColorInfo Colors { get; init; } = new();

    public double DevicePixelRatio { get; init; }

    public IReadOnlyList<DeviceInfo> Devices { get; init; } = [];

    public PluginInfo Plugin { get; init; } = new();
}

public sealed class ApplicationInfo
{
    public string Font { get; init; } = "";

    public string Language { get; init; } = "";

    public string Platform { get; init; } = "";

    public string PlatformVersion { get; init; } = "";

    public string Version { get; init; } = "";
}

public sealed class ColorInfo
{
    public string ButtonMouseOverBackgroundColor { get; init; } = "";

    public string ButtonPressedBackgroundColor { get; init; } = "";

    public string ButtonPressedBorderColor { get; init; } = "";

    public string ButtonPressedTextColor { get; init; } = "";

    public string HighlightColor { get; init; } = "";
}

public sealed class PluginInfo
{
    public string Uuid { get; init; } = "";

    public string Version { get; init; } = "";
}
