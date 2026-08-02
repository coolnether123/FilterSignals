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
$language = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Languages\English\Keyed\FilterSignals.xml'))

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
$indicator = [regex]::Match(
    $filterUi,
    '(?s)internal static void DrawIndicator\(.*?' +
    '(?=\s*private static void DrawToolbar)')
if (-not $indicator.Success)
{
    $failures.Add(
        'Could not isolate status-square click handling.')
}
elseif ($indicator.Value -match 'ClearSelection\(\)')
{
    $failures.Add(
        'A square with no action must leave the storage panel open.')
}
elseif ($indicator.Value -notmatch
    '(?s)ButtonInvisible\(interactionRect\).*?' +
    'if \(target\.IsActionable\).*?' +
    'ClassificationNavigationController\.TryNavigate\(target\)')
{
    $failures.Add(
        'Status-square navigation must run only for an actionable target.')
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
if ($language -notmatch
    '<FilterSignals_CanMake>Able to be produced in this colony</FilterSignals_CanMake>')
{
    $failures.Add(
        'A producible item must use the requested concise colony wording.')
}
$canMakeExplanation = [regex]::Match(
    $presentation,
    '(?s)case ProductionClassification\.CanMakeNow:\s*' +
    'return string\.Empty;')
if (-not $canMakeExplanation.Success)
{
    $failures.Add(
        'A producible item must not add a redundant explanation line.')
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
    'leave storage open, and tooltips omit unavailable-action prose.')
