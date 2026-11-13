param([switch]$Publish)
$ErrorActionPreference = 'Stop'
Push-Location $PSScriptRoot
try {
    dotnet run --project tests/Zentro.Checks -c Release
    if ($LASTEXITCODE -ne 0) { throw 'Workspace checks failed.' }
    if ($Publish) {
        dotnet publish src/Zentro -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -o artifacts/Zentro
    } else {
        dotnet build src/Zentro -c Release
    }
    if ($LASTEXITCODE -ne 0) { throw 'Build failed.' }
} finally { Pop-Location }
