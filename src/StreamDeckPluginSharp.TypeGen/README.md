# StreamDeckPluginSharp.TypeGen

Generates TypeScript interfaces from C# types marked with `[TypeScriptContract]`.

```bash
dotnet tool install -g StreamDeckPluginSharp.TypeGen
sdps-typegen ./MyPlugin.dll ./pi/src/generated/contracts.ts
```
