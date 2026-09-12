param(
    [string]$Configuration = "Release",
    [string[]]$Runtime = @("win-x64", "osx-arm64", "osx-x64"),
    [switch]$Install,
    [switch]$Pack,
    [string]$OutputDir = ""
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $MyInvocation.MyCommand.Path
$pluginId = "dev.flowingspdg.countersample"
$pluginDir = Join-Path $root "$pluginId.sdPlugin"
$piDir = Join-Path $root "pi"
$repoRoot = (Resolve-Path (Join-Path $root "..\..")).Path
if (-not $OutputDir) {
    $OutputDir = Join-Path $repoRoot "artifacts\plugin"
}

function Get-HostRid {
    if ($IsWindows -or $env:OS -like "*Windows*") {
        return "win-x64"
    }
    if ($IsMacOS) {
        if ([System.Runtime.InteropServices.RuntimeInformation]::OSArchitecture.ToString() -eq "Arm64") {
            return "osx-arm64"
        }
        return "osx-x64"
    }
    return "linux-x64"
}

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

    foreach ($rid in $Runtime) {
        $out = Join-Path $pluginDir "bin\$rid"
        if (Test-Path $out) {
            Remove-Item -Recurse -Force $out
        }
        dotnet publish (Join-Path $root "CounterSample.csproj") -c $Configuration -r $rid --self-contained true -p:PublishSingleFile=false -o $out
        Get-ChildItem $out -Filter *.pdb -Recurse | Remove-Item -Force
    }

    Write-Host "Published plugin bundle to $pluginDir"

    if ($Pack) {
        New-Item -ItemType Directory -Force -Path $OutputDir | Out-Null
        $zipPath = Join-Path $OutputDir "$pluginId.streamDeckPlugin"
        if (Test-Path $zipPath) {
            Remove-Item -Force $zipPath
        }
        $cli = Get-Command streamdeck -ErrorAction SilentlyContinue
        if ($cli) {
            & streamdeck pack $pluginDir --output $OutputDir --force
            if ($LASTEXITCODE -ne 0) {
                throw "streamdeck pack failed with exit code $LASTEXITCODE"
            }
        }
        else {
            Add-Type -AssemblyName System.IO.Compression.FileSystem
            [System.IO.Compression.ZipFile]::CreateFromDirectory($pluginDir, $zipPath, [System.IO.Compression.CompressionLevel]::Optimal, $true)
        }
        if (-not (Test-Path $zipPath) -and -not (Get-ChildItem $OutputDir -Filter *.streamDeckPlugin -ErrorAction SilentlyContinue)) {
            throw "Plugin pack was not created in $OutputDir"
        }
        Write-Host "Packed plugin installer in $OutputDir"
    }

    if ($Install) {
        $hostRid = Get-HostRid
        if ($hostRid -notlike "win-*" -and $hostRid -notlike "osx-*") {
            throw "Local Stream Deck install is supported on Windows and macOS only."
        }
        if ($IsWindows -or $env:OS -like "*Windows*") {
            $destRoot = Join-Path $env:APPDATA "Elgato\StreamDeck\Plugins"
        }
        else {
            $destRoot = Join-Path $HOME "Library/Application Support/com.elgato.StreamDeck/Plugins"
        }
        $dest = Join-Path $destRoot "$pluginId.sdPlugin"
        New-Item -ItemType Directory -Force -Path $destRoot | Out-Null
        if (Test-Path $dest) {
            Remove-Item -Recurse -Force $dest
        }
        Copy-Item -Recurse -Force $pluginDir $dest
        Write-Host "Installed plugin to $dest"

        $cli = Get-Command streamdeck -ErrorAction SilentlyContinue
        if ($cli) {
            & streamdeck restart $pluginId
        }
        else {
            Write-Host "Restart Stream Deck (or the plugin) to load the new build."
        }
    }
}
finally {
    Pop-Location
}
