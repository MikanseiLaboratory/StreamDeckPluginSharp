namespace StreamDeckPluginSharp.Commands;

/// <summary>
/// Outgoing WebSocket command names defined by the Stream Deck plugin protocol.
/// </summary>
public static class CommandNames
{
    public const string GetGlobalSettings = "getGlobalSettings";
    public const string GetResources = "getResources";
    public const string GetSecrets = "getSecrets";
    public const string GetSettings = "getSettings";
    public const string LogMessage = "logMessage";
    public const string OpenUrl = "openUrl";
    public const string SendToPropertyInspector = "sendToPropertyInspector";
    public const string SetFeedback = "setFeedback";
    public const string SetFeedbackLayout = "setFeedbackLayout";
    public const string SetGlobalSettings = "setGlobalSettings";
    public const string SetImage = "setImage";
    public const string SetResources = "setResources";
    public const string SetSettings = "setSettings";
    public const string SetState = "setState";
    public const string SetTitle = "setTitle";
    public const string SetTriggerDescription = "setTriggerDescription";
    public const string ShowAlert = "showAlert";
    public const string ShowOk = "showOk";
    public const string SwitchToProfile = "switchToProfile";
}
