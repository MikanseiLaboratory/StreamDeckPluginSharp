using StreamDeckPluginSharp.Connection;
using StreamDeckPluginSharp.TypeGen;
using StreamDeckPluginSharp.Tests.Fakes;

namespace StreamDeckPluginSharp.Tests;

public sealed class RegistrationAndTypeGenTests
{
    [Fact]
    public void Parses_stream_deck_launch_arguments()
    {
        var args = RegistrationArguments.Parse([
            "-port", "28196",
            "-pluginUUID", "ABC123",
            "-registerEvent", "registerPlugin",
            "-info", """{"plugin":{"uuid":"dev.example","version":"1.0.0"},"application":{"platform":"mac"},"devices":[]}"""
        ]);

        Assert.Equal(28196, args.Port);
        Assert.Equal("ABC123", args.PluginUuid);
        Assert.Equal("registerPlugin", args.RegisterEvent);
        Assert.Equal("mac", args.Info.Application.Platform);
        Assert.Equal("dev.example", args.Info.Plugin.Uuid);
    }

    [Fact]
    public void TypeGen_emits_camel_case_interfaces()
    {
        var ts = Program.Generate(typeof(RecordingSettings).Assembly.Location);
        Assert.Contains("export interface RecordingSettings", ts);
        Assert.Contains("increment: number;", ts);
    }

    [Fact]
    public void TypeGen_maps_common_clr_types()
    {
        Assert.Equal("string", Program.ToTypeScript(typeof(string)));
        Assert.Equal("number", Program.ToTypeScript(typeof(int)));
        Assert.Equal("boolean", Program.ToTypeScript(typeof(bool)));
        Assert.Equal("number[]", Program.ToTypeScript(typeof(List<int>)));
        Assert.Equal("increment", Program.Camel("Increment"));
    }
}
