$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$diagnostics = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Diagnostics\TechSenseDebugActions.cs'))

if ($diagnostics -notmatch
    '(?s)"Open small-volume tooltip fixture".*?' +
    'new Dialog_TechSenseFixture\("Gold"\)' -or
    $diagnostics -notmatch
    '(?s)Dialog_TechSenseFixture\(string initialSearch = null\).*?' +
    'uiState\.quickSearch\.filter\.Text = initialSearch')
{
    Write-Error (
        'The automated small-volume fixture must open the ordinary filter ' +
        'dialog pre-filtered to Gold.')
    exit 1
}

Write-Output (
    'PASS: The harness can open a focused small-volume tooltip fixture.')
