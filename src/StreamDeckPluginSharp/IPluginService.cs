namespace StreamDeckPluginSharp;

/// <summary>
/// Optional lifecycle hooks for singleton services that own plugin-wide connections or state.
/// </summary>
public interface IPluginService
{
    Task StartAsync(CancellationToken cancellationToken);

    Task StopAsync(CancellationToken cancellationToken);
}
