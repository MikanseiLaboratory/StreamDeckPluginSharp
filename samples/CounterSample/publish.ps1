param(
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$pluginDir = Join-Path $root "com.flowingspdg.countersample.sdPlugin"
$piDir = Join-Path $root "pi"

Push-Location $root
try {
    dotnet build (Join-Path $root "..\..\src\StreamDeckPluginSharp.TypeGen\StreamDeckPluginSharp.TypeGen.csproj") -c $Configuration
    dotnet build (Join-Path $root "CounterSample.csproj") -c $Configuration

    $typegen = Join-Path $root "..\..\src\StreamDeckPluginSharp.TypeGen\bin\$Configuration\net8.0\StreamDeckPluginSharp.TypeGen.dll"
    $assembly = Join-Path $root "bin\$Configuration\net8.0\dev.flowingspdg.countersample.dll"
    $contracts = Join-Path $piDir "src\generated\contracts.ts"
    dotnet exec $typegen $assembly $contracts

    Push-Location $piDir
    if (-not (Test-Path "node_modules")) {
        npm install
    }
    npm run build
    Pop-Location

    $runtimes = @(
        @{ Rid = "win-x64"; Out = Join-Path $pluginDir "bin\win-x64" },
        @{ Rid = "osx-arm64"; Out = Join-Path $pluginDir "bin\osx-arm64" },
        @{ Rid = "osx-x64"; Out = Join-Path $pluginDir "bin\osx-x64" }
    )

    foreach ($runtime in $runtimes) {
        dotnet publish (Join-Path $root "CounterSample.csproj") -c $Configuration -r $runtime.Rid --self-contained true -o $runtime.Out
    }

    $hostRid = if ($IsWindows -or $env:OS -like "*Windows*") { "win-x64" } elseif ($IsMacOS) {
        if ([System.Runtime.InteropServices.RuntimeInformation]::ProcessArchitecture.ToString() -eq "Arm64") { "osx-arm64" } else { "osx-x64" }
    } else { "win-x64" }
    Copy-Item -Force (Join-Path $pluginDir "bin\$hostRid\*") $pluginDir

    Write-Host "Published plugin bundle to $pluginDir"
}
finally {
    Pop-Location
}
