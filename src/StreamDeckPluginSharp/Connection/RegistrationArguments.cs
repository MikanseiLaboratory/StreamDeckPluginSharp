using StreamDeckPluginSharp.Events;
using StreamDeckPluginSharp.Serialization;

namespace StreamDeckPluginSharp.Connection;

/// <summary>
/// Command-line arguments Stream Deck supplies when launching a native plugin.
/// </summary>
public sealed class RegistrationArguments
{
    public int Port { get; init; }

    public string PluginUuid { get; init; } = "";

    public string RegisterEvent { get; init; } = "";

    public string InfoJson { get; init; } = "";

    public RegistrationInfo Info { get; init; } = new();

    public static RegistrationArguments Parse(IReadOnlyList<string> args)
    {
        var port = 0;
        var pluginUuid = "";
        var registerEvent = "";
        var infoJson = "";

        for (var i = 0; i < args.Count; i++)
        {
            var key = args[i];
            if (i + 1 >= args.Count)
            {
                break;
            }

            var value = args[++i];
            switch (key)
            {
                case "-port":
                    port = int.Parse(value);
                    break;
                case "-pluginUUID":
                    pluginUuid = value;
                    break;
                case "-registerEvent":
                    registerEvent = value;
                    break;
                case "-info":
                    infoJson = value;
                    break;
                default:
                    break;
            }
        }

        if (port <= 0)
        {
            throw new ArgumentException("Stream Deck did not provide a valid -port argument.");
        }

        if (string.IsNullOrWhiteSpace(pluginUuid))
        {
            throw new ArgumentException("Stream Deck did not provide a -pluginUUID argument.");
        }

        if (string.IsNullOrWhiteSpace(registerEvent))
        {
            throw new ArgumentException("Stream Deck did not provide a -registerEvent argument.");
        }

        var info = string.IsNullOrWhiteSpace(infoJson)
            ? new RegistrationInfo()
            : StreamDeckJson.DeserializeRegistrationInfo(infoJson) ?? new RegistrationInfo();

        return new RegistrationArguments
        {
            Port = port,
            PluginUuid = pluginUuid,
            RegisterEvent = registerEvent,
            InfoJson = infoJson,
            Info = info
        };
    }
}
