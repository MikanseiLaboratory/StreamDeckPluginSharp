using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using StreamDeckPluginSharp.Events;
using StreamDeckPluginSharp.Hosting;
using StreamDeckPluginSharp.Serialization;
using StreamDeckPluginSharp.Tests.Fakes;

namespace StreamDeckPluginSharp.Tests;

public sealed class DispatchTests
{
    [Fact]
    public async Task WillAppear_then_keyDown_uses_typed_settings()
    {
        var (dispatcher, connection) = CreateDispatcher();
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "willAppear",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "controller": "Keypad", "settings": { "increment": 5 } }
            }
            """), CancellationToken.None);

        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "keyDown",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "controller": "Keypad", "settings": { "increment": 5 } }
            }
            """), CancellationToken.None);

        var action = GetAction(dispatcher, "ctx");
        Assert.Contains("willAppear", action.Events);
        Assert.Contains("keyDown:5", action.Events);
        Assert.Equal("ctx", action.Context);
        Assert.NotNull(connection);
    }

    [Fact]
    public async Task DidReceiveSettings_updates_typed_settings_and_raises_hook()
    {
        var (dispatcher, _) = CreateDispatcher();
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "willAppear",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "settings": { "increment": 1 } }
            }
            """), CancellationToken.None);

        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "didReceiveSettings",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "settings": { "increment": 9 } }
            }
            """), CancellationToken.None);

        var action = GetAction(dispatcher, "ctx");
        Assert.Equal(9, action.Settings.Increment);
        Assert.Contains("settings:1->9", action.Events);
    }

    [Fact]
    public async Task SendToPlugin_reaches_action()
    {
        var (dispatcher, _) = CreateDispatcher();
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "willAppear",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "settings": {} }
            }
            """), CancellationToken.None);
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "sendToPlugin",
              "action": "dev.test.counter",
              "context": "ctx",
              "payload": { "type": "refresh" }
            }
            """), CancellationToken.None);

        Assert.Contains("pi", GetAction(dispatcher, "ctx").Events);
    }

    [Fact]
    public async Task WillDisappear_disposes_instance()
    {
        var (dispatcher, _) = CreateDispatcher();
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "willAppear",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "settings": {} }
            }
            """), CancellationToken.None);
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "willDisappear",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "settings": {} }
            }
            """), CancellationToken.None);
        await dispatcher.DispatchAsync(Parse("""
            {
              "event": "keyDown",
              "action": "dev.test.counter",
              "context": "ctx",
              "device": "dev",
              "payload": { "settings": {} }
            }
            """), CancellationToken.None);

        var field = typeof(EventDispatcher).GetField("_instances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var instances = (System.Collections.IDictionary)field!.GetValue(dispatcher)!;
        Assert.False(instances.Contains("ctx"));
    }

    private static IncomingMessage Parse(string json) => StreamDeckJson.DeserializeMessage(json)!;

    private static (EventDispatcher Dispatcher, FakeStreamDeckConnection Connection) CreateDispatcher()
    {
        var services = new ServiceCollection().BuildServiceProvider();
        var registry = new ActionRegistry();
        registry.AddFromAssembly(typeof(RecordingAction).Assembly);
        var connection = new FakeStreamDeckConnection();
        var dispatcher = new EventDispatcher(services, registry, connection, NullLogger<EventDispatcher>.Instance);
        return (dispatcher, connection);
    }

    private static RecordingAction GetAction(EventDispatcher dispatcher, string context)
    {
        var field = typeof(EventDispatcher).GetField("_instances", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var instances = (System.Collections.Concurrent.ConcurrentDictionary<string, StreamDeckPluginSharp.Actions.ActionBase>)field!.GetValue(dispatcher)!;
        return (RecordingAction)instances[context];
    }
}
