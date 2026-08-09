$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$fixtureFiles = @(
    'Developer\FilterSignals.TestFixture\README.md',
    'Developer\FilterSignals.TestFixture\About\About.xml',
    'Developer\FilterSignals.TestFixture\Source\FilterSignals.TestFixture.csproj',
    'Developer\FilterSignals.TestFixture\Source\FilterSignalsDebugActions.cs')
foreach ($fixtureFile in $fixtureFiles)
{
    if (-not (Test-Path -LiteralPath (Join-Path $root $fixtureFile) -PathType Leaf))
    {
        Write-Error ('Tracked fixture source is missing: ' + $fixtureFile)
        exit 1
    }
}
$fixtureSource = Join-Path $root $fixtureFiles[2]
$fixtureAssembly = Join-Path $root `
    'Developer\FilterSignals.TestFixture\1.6\Assemblies\FilterSignals.TestFixture.dll'
& git -C $root check-ignore --no-index --quiet -- $fixtureSource 2>$null
if ($LASTEXITCODE -eq 0)
{
    Write-Error 'Fixture project source must be trackable by Git.'
    exit 1
}
& git -C $root check-ignore --no-index --quiet -- $fixtureAssembly 2>$null
if ($LASTEXITCODE -ne 0)
{
    Write-Error 'Generated fixture assemblies must remain ignored.'
    exit 1
}
$diagnostics = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Developer\FilterSignals.TestFixture\Source\FilterSignalsDebugActions.cs'))

if ($diagnostics -notmatch
    '(?s)"Open small-volume tooltip fixture".*?' +
    'new Dialog_FilterSignalsFixture\("Gold"\)' -or
    $diagnostics -notmatch
    '(?s)Dialog_FilterSignalsFixture\(string initialSearch = null\).*?' +
    'uiState\.quickSearch\.filter\.Text = initialSearch')
{
    Write-Error (
        'The automated small-volume fixture must open the ordinary filter ' +
        'dialog pre-filtered to Gold.')
    exit 1
}

Write-Output (
    'PASS: The harness can open a focused small-volume tooltip fixture.')
