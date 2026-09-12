namespace StreamDeckPluginSharp;

/// <summary>
/// Marks a C# type so <c>sdps-typegen</c> can emit a matching TypeScript interface.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum)]
public sealed class TypeScriptContractAttribute : Attribute;
