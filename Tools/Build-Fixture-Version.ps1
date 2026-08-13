param(
    [Parameter(Mandatory = $true)][ValidateSet('1.6','1.5','1.4','1.3','1.2','1.1')]
    [string]$Version,
    [string]$OutputRoot
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

$repository = [System.IO.Path]::GetFullPath((Split-Path -Parent $PSScriptRoot))
$toolingRoot = [Environment]::GetEnvironmentVariable('RWT_CASCADE_TOOLING_ROOT')
if ([string]::IsNullOrWhiteSpace($toolingRoot))
{
    $toolingRoot = 'A:\Dev\RimWorld\Worktrees\RimWorld-Tooling\phase-a'
}

if ([string]::IsNullOrWhiteSpace($OutputRoot))
{
    $OutputRoot = Join-Path $repository "Engineering\fixture-build-$Version"
}

$project = Join-Path $repository 'Developer\FilterSignals.TestFixture\Source\FilterSignals.TestFixture.csproj'
$resultPath = Join-Path $OutputRoot 'build-result.json'
$buildTool = Join-Path $toolingRoot 'tools\Invoke-RimWorldBuild.ps1'

& powershell.exe -NoLogo -NoProfile -ExecutionPolicy Bypass -File $buildTool `
    -Project $project `
    -Configuration $Version `
    -Version $Version `
    -OutputRoot $OutputRoot `
    -Engine MSBuild `
    -Dependency harmony,spine `
    -ResultPath $resultPath | Out-Null

if (-not (Test-Path -LiteralPath $resultPath -PathType Leaf))
{
    throw "No fixture build result was returned for $Version."
}

$result = Get-Content -Raw -LiteralPath $resultPath | ConvertFrom-Json
if (-not [bool]$result.Succeeded)
{
    throw "Filter Signals fixture build failed for $Version."
}

$built = Join-Path $OutputRoot 'build\FilterSignals.TestFixture.dll'
if (-not (Test-Path -LiteralPath $built -PathType Leaf))
{
    throw "Expected fixture assembly was not produced: $built"
}

$payload = Join-Path $repository "Developer\FilterSignals.TestFixture\$Version\Assemblies"
[System.IO.Directory]::CreateDirectory($payload) | Out-Null
[System.IO.File]::Copy(
    $built,
    (Join-Path $payload 'FilterSignals.TestFixture.dll'),
    $true)

$hash = (Get-FileHash -LiteralPath $built -Algorithm SHA256).Hash
[pscustomobject]@{
    Version = $Version
    Assembly = 'FilterSignals.TestFixture.dll'
    Path = (Join-Path $payload 'FilterSignals.TestFixture.dll')
    Sha256 = $hash
    BuildResult = $resultPath
} | ConvertTo-Json -Depth 4
