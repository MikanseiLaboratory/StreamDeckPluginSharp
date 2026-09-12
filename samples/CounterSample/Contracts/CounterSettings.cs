using StreamDeckPluginSharp;

namespace CounterSample.Contracts;

[TypeScriptContract]
public sealed class CounterSettings
{
    public int Increment { get; set; } = 1;
}

[TypeScriptContract]
public sealed class CountChangedMessage
{
    public string Type { get; set; } = "countChanged";

    public int Count { get; set; }
}
