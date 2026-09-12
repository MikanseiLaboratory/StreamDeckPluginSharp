using System.Threading.Channels;
using StreamDeckPluginSharp.Connection;
using StreamDeckPluginSharp.Serialization;

namespace StreamDeckPluginSharp.Tests;

public sealed class ConnectionTests
{
    [Fact]
    public async Task Registers_then_dispatches_inbound_events()
    {
        var transport = new FakeTransport();
        var args = RegistrationArguments.Parse([
            "-port", "12345",
            "-pluginUUID", "uuid-1",
            "-registerEvent", "registerPlugin",
            "-info", "{}"
        ]);
        var connection = new StreamDeckConnection(args, transport);
        var received = new List<string>();

        var run = connection.RunAsync((message, _) =>
        {
            received.Add(message.Event);
            return Task.CompletedTask;
        }, CancellationToken.None);

        await transport.WaitForSentAsync();
        Assert.Contains("registerPlugin", transport.Sent[0]);
        Assert.Contains("uuid-1", transport.Sent[0]);

        await transport.Incoming.Writer.WriteAsync("""{"event":"systemDidWakeUp"}""");
        await transport.Incoming.Writer.WriteAsync("""{"event":"didReceiveDeepLink","payload":{"url":"ping"}}""");
        transport.Incoming.Writer.TryComplete();

        await run;
        Assert.Equal(["systemDidWakeUp", "didReceiveDeepLink"], received);

        var register = StreamDeckJson.DeserializeMessage(transport.Sent[0].Replace("uuid", "action"));
        Assert.NotNull(register);
    }

    private sealed class FakeTransport : IStreamDeckTransport
    {
        public List<string> Sent { get; } = [];

        public Channel<string> Incoming { get; } = Channel.CreateUnbounded<string>();

        private readonly TaskCompletionSource _sent = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public Task ConnectAsync(Uri uri, CancellationToken cancellationToken) => Task.CompletedTask;

        public Task SendAsync(string json, CancellationToken cancellationToken)
        {
            Sent.Add(json);
            _sent.TrySetResult();
            return Task.CompletedTask;
        }

        public async Task<string?> ReceiveAsync(CancellationToken cancellationToken)
        {
            if (!await Incoming.Reader.WaitToReadAsync(cancellationToken))
            {
                return null;
            }

            return Incoming.Reader.TryRead(out var json) ? json : null;
        }

        public Task WaitForSentAsync() => _sent.Task;

        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
