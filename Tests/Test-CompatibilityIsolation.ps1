param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$api = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Compatibility\FilterSignalsApi.cs'))
$service = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Runtime\ClassificationService.cs'))
$failures = [System.Collections.Generic.List[string]]::new()

foreach ($method in @('GetProductionProviders', 'GetClassificationOverrides'))
{
    $match = [regex]::Match(
        $api,
        '(?s)' + [regex]::Escape($method) + '\(\).*?\n\s*\}')
    if (-not $match.Success -or
        $match.Value -notmatch 'OrderBy' -or
        $match.Value -notmatch 'SafeId')
    {
        $failures.Add(
            ($method + ' must expose deterministic safe-ID ordering.'))
    }
}

if ($service -notmatch
    '(?s)EvaluateOverrides\(.*?catch \(Exception exception\)' -or
    $service -notmatch
    '(?s)AddCustomPaths\(.*?catch \(Exception exception\)')
{
    $failures.Add(
        'Provider and override failures must be isolated at their public seams.')
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
    'PASS: provider and override ordering/isolation contracts are present.')
