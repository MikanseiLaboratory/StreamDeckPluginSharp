using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using StreamDeckPluginSharp.Commands;
using StreamDeckPluginSharp.Events;
using StreamDeckPluginSharp.Serialization;

namespace StreamDeckPluginSharp.Connection;

internal sealed class StreamDeckConnection : IStreamDeckConnection, IAsyncDisposable
{
    private readonly RegistrationArguments _arguments;
    private readonly IStreamDeckTransport _transport;
    private readonly ILogger _logger;
    private readonly Channel<string> _outbound = Channel.CreateUnbounded<string>(new UnboundedChannelOptions
    {
        SingleReader = true,
        SingleWriter = false
    });

    public StreamDeckConnection(RegistrationArguments arguments, IStreamDeckTransport? transport = null, ILogger? logger = null)
    {
        _arguments = arguments;
        _transport = transport ?? new ClientWebSocketTransport();
        _logger = logger ?? NullLogger.Instance;
        PluginUuid = arguments.PluginUuid;
        Info = arguments.Info;
    }

    public string PluginUuid { get; }

    public RegistrationInfo Info { get; }

    public async Task RunAsync(Func<IncomingMessage, CancellationToken, Task> onMessage, CancellationToken cancellationToken)
    {
        var uri = new Uri($"ws://127.0.0.1:{_arguments.Port}");
        await _transport.ConnectAsync(uri, cancellationToken).ConfigureAwait(false);
        await SendRawAsync(new RegisterPluginMessage
        {
            Event = _arguments.RegisterEvent,
            Uuid = _arguments.PluginUuid
        }, cancellationToken).ConfigureAwait(false);

        var sender = Task.Run(() => SendLoopAsync(cancellationToken), cancellationToken);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var json = await _transport.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                if (json is null)
                {
                    break;
                }

                IncomingMessage? message;
                try
                {
                    message = StreamDeckJson.DeserializeMessage(json);
                }
                catch (JsonException ex)
                {
                    _logger.LogWarning(ex, "Failed to deserialize Stream Deck message.");
                    continue;
                }

                if (message is null || string.IsNullOrWhiteSpace(message.Event))
                {
                    continue;
                }

                try
                {
                    await onMessage(message, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex) when (ex is not OperationCanceledException)
                {
                    _logger.LogError(ex, "Unhandled exception while dispatching {Event}.", message.Event);
                }
            }
        }
        finally
        {
            _outbound.Writer.TryComplete();
            try
            {
                await sender.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Expected during plugin shutdown.
            }
        }
    }

    public Task SetTitleAsync(string context, string? title, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => SendAsync(new
        {
            @event = CommandNames.SetTitle,
            context,
            payload = new { title, target = (int)target, state }
        }, cancellationToken);

    public Task SetImageAsync(string context, string? image, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
        => SendAsync(new
        {
            @event = CommandNames.SetImage,
            context,
            payload = new { image, target = (int)target, state }
        }, cancellationToken);

    public Task SetImageAsync(string context, byte[] pngBytes, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
    {
        var dataUrl = "data:image/png;base64," + Convert.ToBase64String(pngBytes);
        return SetImageAsync(context, dataUrl, target, state, cancellationToken);
    }

    public async Task SetImageFromFileAsync(string context, string path, Target target = Target.HardwareAndSoftware, int? state = null, CancellationToken cancellationToken = default)
    {
        var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
        var mime = Path.GetExtension(path).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".gif" => "image/gif",
            ".svg" => "image/svg+xml",
            _ => "image/png"
        };
        var dataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";
        await SetImageAsync(context, dataUrl, target, state, cancellationToken).ConfigureAwait(false);
    }

    public Task SetStateAsync(string context, int state, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetState, context, payload = new { state } }, cancellationToken);

    public Task SetSettingsAsync(string context, object settings, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetSettings, context, payload = settings }, cancellationToken);

    public Task GetSettingsAsync(string context, string? id = null, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.GetSettings, context, id }, cancellationToken);

    public Task SetGlobalSettingsAsync(object settings, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetGlobalSettings, context = PluginUuid, payload = settings }, cancellationToken);

    public Task GetGlobalSettingsAsync(string? id = null, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.GetGlobalSettings, context = PluginUuid, id }, cancellationToken);

    public Task ShowOkAsync(string context, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.ShowOk, context }, cancellationToken);

    public Task ShowAlertAsync(string context, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.ShowAlert, context }, cancellationToken);

    public Task SendToPropertyInspectorAsync(string context, object payload, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SendToPropertyInspector, context, payload }, cancellationToken);

    public Task SetFeedbackAsync(string context, object payload, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetFeedback, context, payload }, cancellationToken);

    public Task SetFeedbackLayoutAsync(string context, string layout, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetFeedbackLayout, context, payload = new { layout } }, cancellationToken);

    public Task SetTriggerDescriptionAsync(string context, TriggerDescription description, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetTriggerDescription, context, payload = description }, cancellationToken);

    public Task OpenUrlAsync(string url, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.OpenUrl, payload = new { url } }, cancellationToken);

    public Task LogMessageAsync(string message, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.LogMessage, payload = new { message } }, cancellationToken);

    public Task SwitchToProfileAsync(string device, string? profile = null, int? page = null, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SwitchToProfile, context = PluginUuid, device, payload = new { profile, page } }, cancellationToken);

    public Task GetSecretsAsync(CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.GetSecrets, context = PluginUuid }, cancellationToken);

    public Task GetResourcesAsync(string context, string? id = null, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.GetResources, context, id }, cancellationToken);

    public Task SetResourcesAsync(string context, IReadOnlyDictionary<string, string> resources, CancellationToken cancellationToken = default)
        => SendAsync(new { @event = CommandNames.SetResources, context, payload = resources }, cancellationToken);

    internal Task SendAsync(object command, CancellationToken cancellationToken)
        => SendRawAsync(command, cancellationToken);

    private async Task SendRawAsync(object command, CancellationToken cancellationToken)
    {
        var json = StreamDeckJson.Serialize(command);
        await _outbound.Writer.WriteAsync(json, cancellationToken).ConfigureAwait(false);
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        await foreach (var json in _outbound.Reader.ReadAllAsync(cancellationToken).ConfigureAwait(false))
        {
            await _transport.SendAsync(json, cancellationToken).ConfigureAwait(false);
        }
    }

    public ValueTask DisposeAsync() => _transport.DisposeAsync();
}
