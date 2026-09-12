using StreamDeckPluginSharp.Events;
using StreamDeckPluginSharp.Serialization;

namespace StreamDeckPluginSharp.Tests;

public sealed class SerializationTests
{
    [Fact]
    public void Incoming_keyDown_round_trips_core_fields()
    {
        const string json = """
            {
              "action": "dev.test.counter",
              "context": "ctx-1",
              "device": "dev-1",
              "event": "keyDown",
              "payload": {
                "controller": "Keypad",
                "coordinates": { "column": 2, "row": 1 },
                "isInMultiAction": false,
                "settings": { "increment": 3 }
              }
            }
            """;

        var message = StreamDeckJson.DeserializeMessage(json);
        Assert.NotNull(message);
        Assert.Equal(EventNames.KeyDown, message.Event);
        Assert.Equal("ctx-1", message.Context);
        Assert.Equal(2, message.Payload.GetProperty("coordinates").GetProperty("column").GetInt32());

        var payload = ActionPayloadParser.Parse(message.Payload);
        Assert.Equal(Controller.Keypad, payload.Controller);
        Assert.Equal(2, payload.Coordinates?.Column);
        Assert.Equal(3, payload.Settings.GetProperty("increment").GetInt32());
    }

    [Fact]
    public void Incoming_deviceDidConnect_reads_device_info()
    {
        const string json = """
            {
              "event": "deviceDidConnect",
              "device": "abc",
              "deviceInfo": {
                "name": "Stream Deck XL",
                "size": { "columns": 8, "rows": 4 },
                "type": 2
              }
            }
            """;

        var message = StreamDeckJson.DeserializeMessage(json);
        Assert.NotNull(message);
        Assert.Equal(EventNames.DeviceDidConnect, message.Event);
        Assert.Equal("abc", message.Device);
        Assert.NotNull(message.DeviceInfo);
        Assert.Equal(DeviceType.StreamDeckXl, message.DeviceInfo!.Type);
        Assert.Equal(8, message.DeviceInfo.Size.Columns);
    }

    [Fact]
    public void Incoming_dialRotate_and_deepLink_parse()
    {
        var rotate = StreamDeckJson.DeserializeMessage("""
            {
              "event": "dialRotate",
              "action": "dev.test.dial",
              "context": "c",
              "device": "d",
              "payload": { "controller": "Encoder", "ticks": -2, "pressed": true, "settings": {} }
            }
            """)!;
        var payload = ActionPayloadParser.Parse<DialRotatePayload>(rotate.Payload);
        Assert.Equal(-2, payload.Ticks);
        Assert.True(payload.Pressed);

        var deepLink = StreamDeckJson.DeserializeMessage("""
            { "event": "didReceiveDeepLink", "payload": { "url": "hello/world" } }
            """)!;
        Assert.Equal("hello/world", deepLink.Payload.GetProperty("url").GetString());
    }

    [Fact]
    public void Registration_info_deserializes()
    {
        var info = StreamDeckJson.DeserializeRegistrationInfo("""
            {
              "application": { "language": "en", "platform": "windows", "version": "7.0.0" },
              "devicePixelRatio": 2,
              "devices": [],
              "plugin": { "uuid": "dev.example", "version": "1.0.0" }
            }
            """);

        Assert.NotNull(info);
        Assert.Equal("windows", info!.Application.Platform);
        Assert.Equal("dev.example", info.Plugin.Uuid);
    }

    [Fact]
    public void Command_serialization_uses_event_property()
    {
        var json = StreamDeckJson.Serialize(new { @event = "setTitle", context = "c", payload = new { title = "Hi" } });
        Assert.Contains("\"event\":\"setTitle\"", json.Replace(" ", ""));
        Assert.Contains("\"title\":\"Hi\"", json.Replace(" ", ""));
    }
}
