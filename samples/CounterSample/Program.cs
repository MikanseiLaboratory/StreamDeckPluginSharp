using CounterSample.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StreamDeckPluginSharp;

var builder = StreamDeckPlugin.CreateBuilder(args);
builder.Services.AddSingleton<CounterStore>();
builder.Services.AddLogging(logging => logging.AddConsole());
await using var plugin = builder.Build();
await plugin.RunAsync();
