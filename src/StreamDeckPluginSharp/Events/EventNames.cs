namespace StreamDeckPluginSharp.Events;

/// <summary>
/// Incoming WebSocket event names defined by the Stream Deck plugin protocol.
/// </summary>
public static class EventNames
{
    public const string ApplicationDidLaunch = "applicationDidLaunch";
    public const string ApplicationDidTerminate = "applicationDidTerminate";
    public const string DeviceDidChange = "deviceDidChange";
    public const string DeviceDidConnect = "deviceDidConnect";
    public const string DeviceDidDisconnect = "deviceDidDisconnect";
    public const string DialDown = "dialDown";
    public const string DialRotate = "dialRotate";
    public const string DialUp = "dialUp";
    public const string DidReceiveDeepLink = "didReceiveDeepLink";
    public const string DidReceiveGlobalSettings = "didReceiveGlobalSettings";
    public const string SendToPlugin = "sendToPlugin";
    public const string DidReceiveResources = "didReceiveResources";
    public const string DidReceiveSecrets = "didReceiveSecrets";
    public const string DidReceiveSettings = "didReceiveSettings";
    public const string KeyDown = "keyDown";
    public const string KeyUp = "keyUp";
    public const string PropertyInspectorDidAppear = "propertyInspectorDidAppear";
    public const string PropertyInspectorDidDisappear = "propertyInspectorDidDisappear";
    public const string SystemDidWakeUp = "systemDidWakeUp";
    public const string TitleParametersDidChange = "titleParametersDidChange";
    public const string TouchTap = "touchTap";
    public const string WillAppear = "willAppear";
    public const string WillDisappear = "willDisappear";
}
