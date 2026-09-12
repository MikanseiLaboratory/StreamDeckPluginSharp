namespace StreamDeckPluginSharp;

/// <summary>
/// Marks an action class so the host can register it against a manifest UUID.
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public sealed class StreamDeckActionAttribute : Attribute
{
    public StreamDeckActionAttribute(string uuid)
    {
        if (string.IsNullOrWhiteSpace(uuid))
        {
            throw new ArgumentException("Action UUID is required.", nameof(uuid));
        }

        Uuid = uuid;
    }

    public string Uuid { get; }
}
