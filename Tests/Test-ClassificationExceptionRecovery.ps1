param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$service = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Runtime\ClassificationService.cs'))
$failures = [System.Collections.Generic.List[string]]::new()

$evaluationCatch = [regex]::Match(
    $service,
    '(?s)catch \(Exception exception\)\s*\{.*?' +
    'classification evaluation.*?\$?cacheResult\s*=\s*false;')
if (-not $evaluationCatch.Success)
{
    $failures.Add(
        'Evaluation exceptions must mark their defensive result as non-cacheable.')
}

if ($service -notmatch
    '(?s)if \(cacheResult\)\s*\{\s*state\.Results\.AddOrUpdate\(')
{
    $failures.Add(
        'Only successful evaluation results may enter the classification cache.')
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
    'PASS: evaluation-exception fallbacks are transient and are not cached.')
