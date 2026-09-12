namespace StreamDeckPluginSharp.Connection;

internal interface IStreamDeckTransport : IAsyncDisposable
{
    Task ConnectAsync(Uri uri, CancellationToken cancellationToken);

    Task SendAsync(string json, CancellationToken cancellationToken);

    Task<string?> ReceiveAsync(CancellationToken cancellationToken);
}
