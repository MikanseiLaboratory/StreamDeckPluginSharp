using System.Text.Json;

namespace CounterSample.Services;

internal static class InspectorBridge
{
    public static string ReadCommandType(JsonElement payload)
    {
        if (payload.ValueKind == JsonValueKind.Object && payload.TryGetProperty("type", out var type))
        {
            return type.GetString() ?? "";
        }

        return "";
    }

    public static void Apply(CounterStore store, string commandType, int increment)
    {
        switch (commandType)
        {
            case "reset":
                store.Add(-store.Count);
                break;
            case "add":
                store.Add(increment);
                break;
        }
    }
}
