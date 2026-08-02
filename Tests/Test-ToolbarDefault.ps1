param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$settings = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Settings\FilterSignalsSettings.cs'))
$registry = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Settings\FilterSignalsSettingsRegistry.cs'))
$controller = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Presentation\FilterUiController.cs'))

$failures = [System.Collections.Generic.List[string]]::new()
if ($settings -match
    'ShowClassificationToolbar\s*=\s*true')
{
    $failures.Add(
        'The classification toolbar must be hidden by default.')
}
if ($registry -notmatch
    '"presentation\.toolbar"[\s\S]{0,700}?false,\s*10,')
{
    $failures.Add(
        'The persisted toolbar setting must default to false.')
}
if ($controller -notmatch
    '!settings\.ShowClassificationToolbar' -or
    $controller -notmatch
    '!FilterSignalsSettings\.Current\.ShowClassificationToolbar')
{
    $failures.Add(
        'Hidden toolbar state must reserve no space or filter item rows.')
}
if ($controller -notmatch
    'ShowStatusIndicators')
{
    $failures.Add(
        'Colored status squares must remain independently available.')
}

if ($failures.Count -gt 0)
{
    foreach ($failure in $failures)
    {
        Write-Error $failure
    }
    exit 1
}

Write-Output (
    'PASS: the optional classification toolbar is hidden by default ' +
    'while status squares remain available.')
