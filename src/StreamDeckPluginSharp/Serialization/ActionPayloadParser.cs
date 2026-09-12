using System.Text.Json;
using StreamDeckPluginSharp.Events;

namespace StreamDeckPluginSharp.Serialization;

internal static class ActionPayloadParser
{
    public static ActionPayload Parse(JsonElement payload) => Parse<ActionPayload>(payload);

    public static T Parse<T>(JsonElement payload) where T : ActionPayload, new()
    {
        var result = new T
        {
            Controller = ReadController(payload),
            Coordinates = ReadCoordinates(payload),
            IsInMultiAction = payload.TryGetProperty("isInMultiAction", out var multi) && multi.ValueKind == JsonValueKind.True,
            State = ReadOptionalInt(payload, "state"),
            UserDesiredState = ReadOptionalInt(payload, "userDesiredState"),
            Resources = ReadResources(payload),
            Settings = payload.TryGetProperty("settings", out var settings) ? settings : default
        };

        if (result is DialRotatePayload rotate)
        {
            rotate.Pressed = payload.TryGetProperty("pressed", out var pressed) && pressed.ValueKind == JsonValueKind.True;
            rotate.Ticks = payload.TryGetProperty("ticks", out var ticks) && ticks.TryGetInt32(out var value) ? value : 0;
        }

        if (result is TouchTapPayload tap)
        {
            tap.Hold = payload.TryGetProperty("hold", out var hold) && hold.ValueKind == JsonValueKind.True;
            if (payload.TryGetProperty("tapPos", out var tapPos) && tapPos.ValueKind == JsonValueKind.Array)
            {
                tap.TapX = tapPos.GetArrayLength() > 0 ? tapPos[0].GetInt32() : 0;
                tap.TapY = tapPos.GetArrayLength() > 1 ? tapPos[1].GetInt32() : 0;
            }
        }

        if (result is TitleParametersPayload title)
        {
            title.Title = payload.TryGetProperty("title", out var titleValue) ? titleValue.GetString() ?? "" : "";
            if (payload.TryGetProperty("titleParameters", out var parameters))
            {
                title.TitleParameters = StreamDeckJson.Deserialize<TitleParameters>(parameters) ?? new TitleParameters();
            }
        }

        return result;
    }

    private static Controller ReadController(JsonElement payload)
    {
        if (!payload.TryGetProperty("controller", out var controller))
        {
            return Controller.Keypad;
        }

        return controller.GetString() switch
        {
            "Encoder" => Controller.Encoder,
            "Neo" => Controller.Neo,
            _ => Controller.Keypad
        };
    }

    private static Coordinates? ReadCoordinates(JsonElement payload)
    {
        if (!payload.TryGetProperty("coordinates", out var coordinates) || coordinates.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return new Coordinates
        {
            Column = coordinates.TryGetProperty("column", out var column) ? column.GetInt32() : 0,
            Row = coordinates.TryGetProperty("row", out var row) ? row.GetInt32() : 0
        };
    }

    private static int? ReadOptionalInt(JsonElement payload, string name)
    {
        return payload.TryGetProperty(name, out var value) && value.TryGetInt32(out var parsed)
            ? parsed
            : null;
    }

    private static IReadOnlyDictionary<string, string> ReadResources(JsonElement payload)
    {
        if (!payload.TryGetProperty("resources", out var resources) || resources.ValueKind != JsonValueKind.Object)
        {
            return new Dictionary<string, string>();
        }

        var map = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var property in resources.EnumerateObject())
        {
            map[property.Name] = property.Value.GetString() ?? "";
        }

        return map;
    }
}
