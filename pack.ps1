param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$nugetOut = Join-Path $root "artifacts\nuget"
$pluginOut = Join-Path $root "artifacts\plugin"

New-Item -ItemType Directory -Force -Path $nugetOut | Out-Null
dotnet pack (Join-Path $root "src\StreamDeckPluginSharp\StreamDeckPluginSharp.csproj") -c $Configuration -o $nugetOut
dotnet pack (Join-Path $root "src\StreamDeckPluginSharp.TypeGen\StreamDeckPluginSharp.TypeGen.csproj") -c $Configuration -o $nugetOut

& (Join-Path $root "samples\CounterSample\publish.ps1") -Configuration $Configuration -Pack -OutputDir $pluginOut

Write-Host "NuGet packages: $nugetOut"
Write-Host "Plugin package: $pluginOut"
