param()

$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$filterUi = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Presentation\FilterUiController.cs'))
$rowPatch = [System.IO.File]::ReadAllText(
    (Join-Path $root 'Source\Patches\ThingFilterUiPatches.cs'))
$language = [System.IO.File]::ReadAllText(
    (Join-Path $root `
        'Languages\English\Keyed\FilterSignals.xml'))

$indicator = [regex]::Match(
    $filterUi,
    '(?s)internal static void DrawIndicator\(.*?' +
    '(?=\s*private static void DrawToolbar)')
if (-not $indicator.Success)
{
    Write-Error 'Could not isolate the status-indicator UI boundary.'
    exit 1
}

$clear = $indicator.Value.IndexOf(
    'TooltipHandler.ClearTooltipsFrom(interactionRect)',
    [System.StringComparison]::Ordinal)
$tip = $indicator.Value.IndexOf(
    'TooltipHandler.TipRegion(',
    [System.StringComparison]::Ordinal)

if ($clear -lt 0 -or $tip -lt 0 -or $clear -gt $tip)
{
    Write-Error (
        'The status square must clear the overlapping vanilla tooltip ' +
        'before registering its own tooltip.')
    exit 1
}
if ($filterUi -notmatch
    'private const float StatusIndicatorRightInset = 45f' -or
    $indicator.Value -notmatch
    'listing\.ColumnWidth - StatusIndicatorRightInset')
{
    Write-Error (
        'The status-square column must use the space freed by the hidden ' +
        'small-volume marker.')
    exit 1
}
if ($rowPatch -notmatch
    'ref List<ThingDef> ___suppressSmallVolumeTags' -or
    $rowPatch -notmatch
    '___suppressSmallVolumeTags\.Add\(tDef\)' -or
    $rowPatch -notmatch
    '(?s)private static Exception Finalizer\(.*?' +
    'RestoreSmallVolumeSuppression')
{
    Write-Error (
        'The vanilla /10 marker must be suppressed only during the patched ' +
        'item row and restored even when drawing fails.')
    exit 1
}
if ($indicator.Value -notmatch
    '(?s)thingDef\.IsStuff && thingDef\.smallVolume.*?' +
    '"FilterSignals_SmallVolume"\.Translate\(\)' -or
    $language -notmatch
    '<FilterSignals_SmallVolume>Small-volume: 10 units = 1\.</FilterSignals_SmallVolume>')
{
    Write-Error (
        'The square tooltip must carry the concise small-volume detail.')
    exit 1
}

Write-Output (
    'PASS: The status square owns its hover area without stacking the ' +
    'vanilla item tooltip; /10 is hidden and explained on square hover.')
