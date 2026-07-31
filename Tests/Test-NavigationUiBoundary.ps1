param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$controller = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Source\Presentation\ClassificationNavigationController.cs'))
$filterUi = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Presentation\FilterUiController.cs'))
$presentation = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Source\Presentation\ClassificationPresentation.cs'))

$failures = [System.Collections.Generic.List[string]]::new()
$buildOption = [regex]::Match(
    $controller,
    '(?s)private static bool SelectBuildOption\(.*?' +
    '(?=\s*private static Designator_Build FindBuildDesignator)')
if (-not $buildOption.Success)
{
    $failures.Add('Could not isolate Architect navigation.')
}
elseif ($buildOption.Value -match 'CameraJumper')
{
    $failures.Add(
        'Architect navigation must not move or recenter the camera.')
}
if ($controller -notmatch 'Find\.CurrentMap\s*!=\s*target\.Map')
{
    $failures.Add(
        'Architect navigation must fail safely for a non-current map.')
}
if ($filterUi -notmatch
    '(?s)!target\.IsActionable.*?Find\.Selector\.ClearSelection\(\)')
{
    $failures.Add(
        'A square with no action must close the storage panel.')
}
if ($presentation -match 'NavigationUnavailable' -or
    $presentation -match 'NavigationChoiceRule' -or
    $presentation -match 'SelectedPath')
{
    $failures.Add(
        'The square tooltip must not explain unavailable navigation or ' +
        'internal target-selection rules.')
}
if ($presentation -notmatch
    'default:\s*\r?\n\s*return string\.Empty')
{
    $failures.Add(
        'A square with no navigation action must add no navigation text.')
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
    'PASS: Architect navigation preserves the camera, no-target clicks ' +
    'close storage, and tooltips omit unavailable-action prose.')
