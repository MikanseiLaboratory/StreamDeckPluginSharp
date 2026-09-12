using System.Text.Json;
using System.Text.Json.Serialization;
using StreamDeckPluginSharp.Events;

namespace StreamDeckPluginSharp.Serialization;

internal static class StreamDeckJson
{
    public static readonly JsonSerializerOptions Options = CreateOptions();

    public static IncomingMessage? DeserializeMessage(string json)
    {
        return JsonSerializer.Deserialize(json, StreamDeckJsonContext.Default.IncomingMessage);
    }

    public static RegistrationInfo? DeserializeRegistrationInfo(string json)
    {
        return JsonSerializer.Deserialize(json, StreamDeckJsonContext.Default.RegistrationInfo);
    }

    public static T? Deserialize<T>(JsonElement element)
    {
        return element.ValueKind is JsonValueKind.Undefined or JsonValueKind.Null
            ? default
            : element.Deserialize<T>(Options);
    }

    public static string Serialize(object value)
    {
        return JsonSerializer.Serialize(value, value.GetType(), Options);
    }

    private static JsonSerializerOptions CreateOptions()
    {
        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };
        options.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
        return options;
    }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true)]
[JsonSerializable(typeof(IncomingMessage))]
[JsonSerializable(typeof(RegistrationInfo))]
[JsonSerializable(typeof(DeviceInfo))]
[JsonSerializable(typeof(DeviceSize))]
[JsonSerializable(typeof(Coordinates))]
[JsonSerializable(typeof(ActionPayload))]
[JsonSerializable(typeof(DialRotatePayload))]
[JsonSerializable(typeof(TouchTapPayload))]
[JsonSerializable(typeof(TitleParametersPayload))]
[JsonSerializable(typeof(TitleParameters))]
[JsonSerializable(typeof(ApplicationEventPayload))]
[JsonSerializable(typeof(DeepLinkPayload))]
[JsonSerializable(typeof(SettingsEnvelope))]
[JsonSerializable(typeof(SecretsEnvelope))]
[JsonSerializable(typeof(RegisterPluginMessage))]
internal partial class StreamDeckJsonContext : JsonSerializerContext;

internal sealed class RegisterPluginMessage
{
    public required string Event { get; init; }

    public required string Uuid { get; init; }
}
